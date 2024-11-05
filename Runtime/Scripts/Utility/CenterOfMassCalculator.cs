using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace jp.go.aist3ddbclient
{
    public class CenterOfMassCalculator
    {
        public static (double, double) GetCenterOfMass(List<(double X, double Y)> coordinates)
        {
            // 座標が2つ以下の場合は計算できないので最初の１点を返却
            if (coordinates.Count < 3) {
                throw new ArgumentException("At least 3 points are required to compute the center of mass.");
            }

            // 座標の中心を計算するために中間変数を設定
            double sx = 0, sy = 0, sArea = 0;

            // 最初の点で閉じるために、座標の最後に最初の座標を追加
            coordinates.Add(coordinates[0]);

            // 中心化のために最初の重心を取得（すべてのポイントを 0,0 に移動させる）
            var translation = GetCentroid(coordinates);

            // 中心化された座標リストを作成
            var neutralizedPoints = new List<(double X, double Y)>();
            foreach (var point in coordinates)
            {
                neutralizedPoints.Add((point.X - translation.Item1, point.Y - translation.Item2));
            }

            // 計算処理
            for (int i = 0; i < neutralizedPoints.Count - 1; i++)
            {
                var pi = neutralizedPoints[i];
                var pj = neutralizedPoints[i + 1];

                var xi = pi.X;
                var yi = pi.Y;
                var xj = pj.X;
                var yj = pj.Y;

                var a = xi * yj - xj * yi; // 共通因子
                sArea += a;                // 面積の合計
                sx += (xi + xj) * a;       // x座標の合計
                sy += (yi + yj) * a;       // y座標の合計
            }

            // 面積が0の場合、重心でフォールバック
            if (sArea == 0)
            {
                return translation;
            }

            // 面積を0.5倍して1/6Aを計算
            var area = sArea * 0.5;
            var areaFactor = 1 / (6 * area);

            // 中心化された座標に値を戻して最終結果を計算
            var centerX = translation.Item1 + areaFactor * sx;
            var centerY = translation.Item2 + areaFactor * sy;

            return (centerX, centerY);
        }

        // ポリゴンの単純な重心を計算するヘルパー
        public static (double, double) GetCentroid(List<(double X, double Y)> coordinates)
        {
            double sumX = 0, sumY = 0;
            foreach (var coord in coordinates)
            {
                sumX += coord.X;
                sumY += coord.Y;
            }
            return (sumX / coordinates.Count, sumY / coordinates.Count);
        }

        public static void ProcessPolygon(List<(double X, double Y)> result, JArray coordinates)
        {
            foreach (var polygon in coordinates)
            {
                foreach (var point in polygon)
                {
                    var x = ((JArray)point)[0].Value<double>();
                    var y = ((JArray)point)[1].Value<double>();
                    result.Add((x, y));
                }
            }
        }

        public static void ProcessMultiPolygon(List<(double X, double Y)> result, JArray coordinates)
        {
            foreach (var multiPolygon in coordinates)
            {
                ProcessPolygon(result, (JArray)multiPolygon);
                // foreach (var polygon in multiPolygon)
                // {
                //     foreach (var point in polygon)
                //     {
                //         result.Add((point[0], point[1]));
                //     }
                // }
            }
        }
    }
}