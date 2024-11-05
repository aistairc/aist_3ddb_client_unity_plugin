using UnityEngine;
using Unity.Mathematics;
using CesiumForUnity;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace jp.go.aist3ddbclient
{
    /// <summary>
    /// A camera controller that can easily move around and view the globe while 
    /// maintaining a sensible orientation. As the camera moves across the horizon, 
    /// it automatically changes its own up direction such that the world always 
    /// looks right-side up.
    /// </summary>
    [RequireComponent(typeof(CesiumOriginShift))]
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class AistCameraController : MonoBehaviour
    {
        #region User-editable properties

        [SerializeField]
        private bool _enableMovement = true;

        [SerializeField]
        private bool _enableRotation = true;

        [SerializeField]
        [Min(0.0f)]
        private float _dynamicSpeedMinHeight = 20.0f;

        /// <summary>
        /// The minimum height where dynamic speed starts to take effect.
        /// Below this height, the speed will be set to the object's height 
        /// from the Earth, which makes it move slowly when it is right above a tileset.
        /// </summary>
        public float dynamicSpeedMinHeight
        {
            get => _dynamicSpeedMinHeight;
            set => _dynamicSpeedMinHeight = Mathf.Max(value, 0.0f);
        }

        [SerializeField]
        private bool _enableDynamicClippingPlanes = true;

        /// <summary>
        /// Whether to dynamically adjust the camera's clipping planes so that
        /// the globe will not be clipped from far away. Objects that are close 
        /// to the camera but far above the globe in space may not appear.
        /// </summary>
        public bool enableDynamicClippingPlanes
        {
            get => _enableDynamicClippingPlanes;
            set => _enableDynamicClippingPlanes = value;
        }

        [SerializeField]
        [Min(0.0f)]
        private float _dynamicClippingPlanesMinHeight = 10000.0f;

        /// <summary>
        /// The height to start dynamically adjusting the camera's clipping planes. 
        /// Below this height, the clipping planes will be set to their initial values.
        /// </summary>
        public float dynamicClippingPlanesMinHeight
        {
            get => _dynamicClippingPlanesMinHeight;
            set => _dynamicClippingPlanesMinHeight = Mathf.Max(value, 0.0f);
        }


        [SerializeField]
        private float _maxSpeed = 100.0f; // Maximum speed with the speed multiplier applied.

        [SerializeField]
        private float _lookSpeed = 50.0f;

        #endregion

        #region Private variables

        private Camera _camera;
        private float _initialNearClipPlane;
        private float _initialFarClipPlane;

        private CharacterController _controller;
        private CesiumGeoreference _georeference;
        private CesiumGlobeAnchor _globeAnchor;

        private Vector3 _velocity = Vector3.zero;

        // These numbers are borrowed from Cesium for Unreal.
        private float _acceleration = 20000.0f;
        private float _deceleration = 9999999959.0f;
        private float _maxRaycastDistance = 1000 * 1000; // 1000 km;
        private float _maxSpeedPreMultiplier = 0.0f; // Max speed without the multiplier applied.
        private AnimationCurve _maxSpeedCurve;

        private float _speedMultiplier = 1.0f;
        private float _maximumNearClipPlane = 1000.0f;
        private float _maximumFarClipPlane = 500000000.0f;

        private float _maximumNearToFarRatio = 100000.0f;

        #endregion

        // Raycast用のリストを保持します
        private PointerEventData pointerData;
        private List<RaycastResult> raycastResults;

        // UIレイヤーのレイヤーマスク
        public LayerMask uiLayerMask = 1 << 5;

        #region Input configuration

#if ENABLE_INPUT_SYSTEM

        void ConfigureInputs()
        {
            InputActionMap map = new InputActionMap("Aist Camera Controller");
        }
#endif

        #endregion

        #region Initialization

        void InitializeCamera()
        {
            _camera = gameObject.GetComponent<Camera>();
            _initialNearClipPlane = _camera.nearClipPlane;
            _initialFarClipPlane = _camera.farClipPlane;
        }

        void InitializeController()
        {
            if (gameObject.GetComponent<CharacterController>() != null)
            {
                Debug.LogWarning(
                    "A CharacterController component was manually " +
                    "added to the CesiumCameraController's game object. " +
                    "This may interfere with the CesiumCameraController's movement.");

                _controller = gameObject.GetComponent<CharacterController>();
            }
            else
            {
                _controller = gameObject.AddComponent<CharacterController>();
                _controller.hideFlags = HideFlags.HideInInspector;
            }

            _controller.radius = 1.0f;
            _controller.height = 1.0f;
            _controller.center = Vector3.zero;
            _controller.detectCollisions = true;
        }

        /// <summary>
        /// Creates a curve to control the bounds of the maximum speed before it is
        /// multiplied by the speed multiplier. This prevents the camera from achieving 
        /// an unreasonably low or high speed.
        /// </summary>
        private void CreateMaxSpeedCurve()
        {
            // This creates a curve that is linear between the first two keys,
            // then smoothly interpolated between the last two keys.
            Keyframe[] keyframes = {
                new Keyframe(0.0f, 4.0f),
                new Keyframe(10000000.0f, 10000000.0f),
                new Keyframe(13000000.0f, 2000000.0f)
            };

            keyframes[0].weightedMode = WeightedMode.Out;
            keyframes[0].outTangent = keyframes[1].value / keyframes[0].value;
            keyframes[0].outWeight = 0.0f;

            keyframes[1].weightedMode = WeightedMode.In;
            keyframes[1].inWeight = 0.0f;
            keyframes[1].inTangent = keyframes[1].value / keyframes[0].value;
            keyframes[1].outTangent = 0.0f;

            keyframes[2].inTangent = 0.0f;

            _maxSpeedCurve = new AnimationCurve(keyframes);
            _maxSpeedCurve.preWrapMode = WrapMode.ClampForever;
            _maxSpeedCurve.postWrapMode = WrapMode.ClampForever;
        }

        void Awake()
        {
            _georeference = gameObject.GetComponentInParent<CesiumGeoreference>();
            if (_georeference == null)
            {
                Debug.LogError(
                    "CesiumCameraController must be nested under a game object " +
                    "with a CesiumGeoreference.");
            }

            // CesiumOriginShift will add a CesiumGlobeAnchor automatically.
            _globeAnchor = gameObject.GetComponent<CesiumGlobeAnchor>();

            InitializeCamera();
            InitializeController();
            CreateMaxSpeedCurve();

            // EventSystemのインスタンスを取得
            pointerData = new PointerEventData(EventSystem.current);
            raycastResults = new List<RaycastResult>();


#if ENABLE_INPUT_SYSTEM
            ConfigureInputs();
#endif
        }

        #endregion

        // マウスカーソルの下にUIがあるか判定するメソッド
        public bool IsPointerOverUI()
        {
            // マウス位置をpointerDataに設定
            pointerData.position = Input.mousePosition;

            // 現在のRaycasterでUIをRaycast
            raycastResults.Clear();
            EventSystem.current.RaycastAll(pointerData, raycastResults);

            // Raycast結果をループして特定のレイヤーに属するか確認
            foreach (RaycastResult result in raycastResults)
            {
                // レイヤーが「UI」レイヤーマスクと一致するかを確認
                if (uiLayerMask == (uiLayerMask | (1 << result.gameObject.layer)))
                {
                    return true;  // UIレイヤーの要素が見つかった
                }
            }

            // UIレイヤーの要素が見つからなかった
            return false;
        }

        #region Update

        void Update()
        {
            HandlePlayerInputs();

            if (_enableDynamicClippingPlanes)
            {
                UpdateClippingPlanes();
            }
        }

        #endregion

        #region Raycasting helpers

        private bool RaycastTowardsEarthCenter(out float hitDistance)
        {
            double3 center =
                _georeference.TransformEarthCenteredEarthFixedPositionToUnity(new double3(0.0));

            RaycastHit hitInfo;
            if (Physics.Linecast(transform.position, (float3)center, out hitInfo))
            {
                hitDistance = Vector3.Distance(transform.position, hitInfo.point);
                return true;
            }

            hitDistance = 0.0f;
            return false;
        }

        private bool RaycastAlongForwardVector(float raycastDistance, out float hitDistance)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(
                transform.position,
                transform.forward,
                out hitInfo,
                raycastDistance))
            {
                hitDistance = Vector3.Distance(transform.position, hitInfo.point);
                return true;
            }

            hitDistance = 0.0f;
            return false;
        }

        #endregion

        #region Player movement

        private void HandlePlayerInputs()
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 lookDelta;
            Vector2 moveDelta;
            lookDelta = Mouse.current.middleButton.isPressed ? Mouse.current.delta.ReadValue() : Vector2.zero;
            moveDelta = Mouse.current.leftButton.isPressed ? Mouse.current.delta.ReadValue() : Vector2.zero;

            float inputRotateHorizontal = lookDelta.x;
            float inputRotateVertical = lookDelta.y;

            float inputForward = -moveDelta.y;
            float inputRight = -moveDelta.x;

            float inputUp = Mouse.current.rightButton.isPressed ? Mouse.current.delta.ReadValue().y : 0.0f;

#endif
            Vector3 movementInput = new Vector3(inputRight, inputUp, inputForward);

            if (_enableRotation)
            {
                Rotate(inputRotateHorizontal, inputRotateVertical);
            }

            if (_enableMovement)
            {
                Move2(movementInput);
            }
        }

        /// <summary>
        /// Rotate the camera with the specified amounts.
        /// </summary>
        /// <remarks>
        /// Horizontal rotation (i.e. looking left or right) corresponds to rotation around the Y-axis.
        /// Vertical rotation (i.e. looking up or down) corresponds to rotation around the X-axis.
        /// </remarks>
        /// <param name="horizontalRotation">The amount to rotate horizontally, i.e. around the Y-axis.</param>
        /// <param name="verticalRotation">The amount to rotate vertically, i.e. around the X-axis.</param>
        private void Rotate(float horizontalRotation, float verticalRotation)
        {
            if (horizontalRotation == 0.0f && verticalRotation == 0.0f)
            {
                return;
            }

            float valueX = verticalRotation * _lookSpeed * Time.smoothDeltaTime;
            float valueY = horizontalRotation * _lookSpeed * Time.smoothDeltaTime;

            // Rotation around the X-axis occurs counter-clockwise, so the look range
            // maps to [270, 360] degrees for the upper quarter-sphere of motion, and
            // [0, 90] degrees for the lower. Euler angles only work with positive values,
            // so map the [0, 90] range to [360, 450] so the entire range is [270, 450].
            // This makes it easy to clamp the values.
            float rotationX = transform.localEulerAngles.x;
            if (rotationX <= 90.0f)
            {
                rotationX += 360.0f;
            }

            float newRotationX = Mathf.Clamp(rotationX - valueX, 270.0f, 450.0f);
            float newRotationY = transform.localEulerAngles.y + valueY;
            transform.localRotation =
                Quaternion.Euler(newRotationX, newRotationY, transform.localEulerAngles.z);
        }

        /// <summary>
        /// Moves the controller with the given player input.
        /// </summary>
        /// <remarks>
        /// The x-coordinate affects movement along the transform's right axis.
        /// The y-coordinate affects movement along the georeferenced up axis.
        /// The z-coordinate affects movement along the transform's forward axis.
        /// </remarks>
        /// <param name="movementInput">The player input.</param>
        private void Move2(Vector3 movementInput)
        {
            Vector3 inputDirection = transform.TransformDirection(movementInput);
            if (Mouse.current.rightButton.isPressed)
            {
                inputDirection.x = 0.0f;
                inputDirection.z = 0.0f;
            }
            else
            {
                inputDirection.y = 0.0f;
            }
            transform.position += inputDirection * _maxSpeed * Time.deltaTime;

            if (!_globeAnchor.detectTransformChanges)
            {
                _globeAnchor.Sync();
            }
        }

        /// <summary>
        /// Moves the controller with the given player input.
        /// </summary>
        /// <remarks>
        /// The x-coordinate affects movement along the transform's right axis.
        /// The y-coordinate affects movement along the georeferenced up axis.
        /// The z-coordinate affects movement along the transform's forward axis.
        /// </remarks>
        /// <param name="movementInput">The player input.</param>
        private void Move(Vector3 movementInput)
        {
            Vector3 inputDirection =
                transform.right * movementInput.x + transform.forward * movementInput.z;

            //Vector3 inputDirection = movementInput;

            if (_georeference != null)
            {
                double3 positionECEF = _globeAnchor.positionGlobeFixed;
                double3 upECEF = CesiumWgs84Ellipsoid.GeodeticSurfaceNormal(positionECEF);
                double3 upUnity =
                    _georeference.TransformEarthCenteredEarthFixedDirectionToUnity(upECEF);

                inputDirection = (float3)inputDirection + (float3)upUnity * movementInput.y;
            }

            if (inputDirection != Vector3.zero)
            {
                // If the controller was already moving, handle the direction change
                // separately from the magnitude of the velocity.
                if (_velocity.magnitude > 0.0f)
                {
                    Vector3 directionChange = inputDirection - _velocity.normalized;
                    _velocity +=
                        directionChange * _velocity.magnitude * Time.deltaTime;
                }

                _velocity += inputDirection * _acceleration * Time.deltaTime;
                _velocity = Vector3.ClampMagnitude(_velocity, _maxSpeed);
            }
            else
            {
                // Decelerate
                float speed = Mathf.Max(
                    _velocity.magnitude - _deceleration * Time.deltaTime,
                    0.0f);

                _velocity = Vector3.ClampMagnitude(_velocity, speed);
            }

            if (_velocity != Vector3.zero)
            {
                _controller.Move(_velocity * Time.deltaTime);

                // Other controllers may disable detectTransformChanges to control their own
                // movement, but the globe anchor should be synced even if detectTransformChanges
                // is false.
                if (!_globeAnchor.detectTransformChanges)
                {
                    _globeAnchor.Sync();
                }
            }
        }

        #endregion

        #region Dynamic speed computation

        /// <summary>
        /// Gets the dynamic speed of the controller based on the camera's height from 
        /// the earth's center and its distance from objects along the forward vector.
        /// </summary>
        /// <param name="overrideSpeed">Whether the returned speed should override the 
        /// previous speed, even if the new value is lower.</param>
        /// <param name="newSpeed">The new dynamic speed of the controller.</param>
        /// <returns>Whether a valid speed value was found.</returns>
        private bool GetDynamicSpeed(out bool overrideSpeed, out float newSpeed)
        {
            if (_georeference == null)
            {
                overrideSpeed = false;
                newSpeed = 0.0f;

                return false;
            }

            float height, viewDistance;

            // Raycast from the camera to the Earth's center and compute the distance.
            // Ignore the result if the height is approximately 0.
            if (!RaycastTowardsEarthCenter(out height) || height < 0.000001f)
            {
                overrideSpeed = false;
                newSpeed = 0.0f;

                return false;
            }

            // Also ignore the result if the speed will increase or decrease by too much at once.
            // This can be an issue when 3D tiles are loaded/unloaded from the scene.
            if (_maxSpeedPreMultiplier > 0.5f)
            {
                float heightToMaxSpeedRatio = height / _maxSpeedPreMultiplier;

                // The asymmetry of these ratios is intentional. When traversing tilesets
                // with many height differences (e.g. a city with tall buildings), flying over
                // taller geometry will cause the camera to slow down suddenly, and sometimes
                // cause it to stutter.
                if (heightToMaxSpeedRatio > 1000.0f || heightToMaxSpeedRatio < 0.01f)
                {
                    overrideSpeed = false;
                    newSpeed = 0.0f;

                    return false;
                }
            }

            // Raycast along the camera's view (forward) vector.
            float raycastDistance =
                Mathf.Clamp(_maxSpeed * 3.0f, 0.0f, _maxRaycastDistance);

            // If the raycast does not hit, then only override speed if the height
            // is lower than the maximum threshold. Otherwise, if both raycasts hit,
            // always override the speed.
            if (!RaycastAlongForwardVector(raycastDistance, out viewDistance) ||
                viewDistance < 0.000001f)
            {
                overrideSpeed = height <= _dynamicSpeedMinHeight;
            }
            else
            {
                overrideSpeed = true;
            }

            // Set the speed to be the height of the camera from the Earth's center.
            newSpeed = height;

            return true;
        }

        private void ResetSpeedMultiplier()
        {
            _speedMultiplier = 1.0f;
        }

        private void SetMaxSpeed(float speed)
        {
            float actualSpeed = _maxSpeedCurve.Evaluate(speed);
            _maxSpeed = _speedMultiplier * actualSpeed;
            _acceleration =
                Mathf.Clamp(_maxSpeed * 5.0f, 20000.0f, 10000000.0f);
        }

        private void UpdateDynamicSpeed()
        {
            bool overrideSpeed;
            float newSpeed;
            if (GetDynamicSpeed(out overrideSpeed, out newSpeed))
            {
                if (overrideSpeed || newSpeed >= _maxSpeedPreMultiplier)
                {
                    _maxSpeedPreMultiplier = newSpeed;
                }
            }

            SetMaxSpeed(_maxSpeedPreMultiplier);
        }

        private void ResetSpeed()
        {
            // this._maxSpeed = this._defaultMaximumSpeed;
            // this._maxSpeedPreMultiplier = 0.0f;
            // this.ResetSpeedMultiplier();
        }

        #endregion

        #region Dynamic clipping plane adjustment

        private void UpdateClippingPlanes()
        {
            if (_camera == null)
            {
                return;
            }

            // Raycast from the camera to the Earth's center and compute the distance.
            float height = 0.0f;
            if (!RaycastTowardsEarthCenter(out height))
            {
                return;
            }

            float nearClipPlane = _initialNearClipPlane;
            float farClipPlane = _initialFarClipPlane;

            if (height >= _dynamicClippingPlanesMinHeight)
            {
                farClipPlane = height + (float)(2.0 * CesiumWgs84Ellipsoid.GetMaximumRadius());
                farClipPlane = Mathf.Min(farClipPlane, _maximumFarClipPlane);

                float farClipRatio = farClipPlane / _maximumNearToFarRatio;

                if (farClipRatio > nearClipPlane)
                {
                    nearClipPlane = Mathf.Min(farClipRatio, _maximumNearClipPlane);
                }
            }

            _camera.nearClipPlane = nearClipPlane;
            _camera.farClipPlane = farClipPlane;
        }

        #endregion
    }
}
