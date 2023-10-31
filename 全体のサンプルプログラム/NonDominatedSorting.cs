//「非支配ソート」・「混雑度の計算」を行うクラス

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public static class NonDominatedSorting
{

    public static List<int> Sort(List<Sample_Solution> solutions) //各要素の値を取得
    {
        int N = solutions.Count;  //候補解の数をカウント、
        List<int> frontLevels = new List<int>(new int[N]); //支配ソートレベル
        List<int>[] dominatedBy = new List<int>[N]; //解iを支配するすべての解のインデックスを保存します。
        int[] numDominated = new int[N]; //解iを支配する他の解の数

        //候補解の数ごとにループ
        for (int i = 0; i < N; i++)
        {
            dominatedBy[i] = new List<int>();
            //要素ごとに２周目のループ
            for (int j = 0; j < N; j++)
            {
                if (i != j)
                {
                    // iがjに支配されるかどうかを確認します。
                    if (Dominates(solutions[j], solutions[i])) //もしiがjを支配していたら
                    {
                        dominatedBy[i].Add(j); //解iを支配するすべての解のインデックスを保存する
                    }
                    else if (Dominates(solutions[i], solutions[j])) //もしiがjに支配されていたら
                    {
                        numDominated[i]++; //解iが支配される他の解の数を保存します。
                    }
                }
            }

            //被支配数がゼロならランク１（,多分ランクが高い＝値が小さい、ランクが低い：値が大きい）ランク１が最高
            if (numDominated[i] == 0)
            {
                frontLevels[i] = 1;
            }
        }

        for (int i = 0; i < N; i++)
        {
            foreach (int j in dominatedBy[i]) //解iが支配するすべての解jに対してループを回す
            {
                numDominated[j]--; //解jが支配されている数を1つ減少させます。これは、解iが解jを支配することが確認されたためです。
                if (numDominated[j] == 0)
                {
                    frontLevels[j] = frontLevels[i] + 1;
                }
            }
        }

        return frontLevels;
    }


    ///混雑度の計算
    public static List<double> CalculateCrowdingDistance(List<Sample_Solution> front)
    {
        int size = front.Count; //フロントの解の数

        // 各解の混雑度を保存するためのリストを初期化
        List<double> crowdingDistance = new List<double>(new double[size]);

        // フロントに解がない場合
        if (size == 0)
            return crowdingDistance;

        // フロントに1つの解のみがある場合混雑度は無限大
        if (size == 1)
        {
            crowdingDistance[0] = double.PositiveInfinity;  //正の無限大
            return crowdingDistance;
        }

        // フロントに2つの解がある場合、混雑度は無限大
        if (size == 2)
        {
            crowdingDistance[0] = double.PositiveInfinity;
            crowdingDistance[1] = double.PositiveInfinity;
            return crowdingDistance;
        }


        //目的関数ごとに解をソート
        List<int>[] sortedIndicesByObjective = new List<int>[3];  // 各目的関数ごとに解をソートするためのインデックスを保存
        for (int i = 0; i < 3; i++)
        {
            sortedIndicesByObjective[i] = SortByObjective(front, i); //目的関数ごとに解をソート
                                                                     //各目的関数においてソートされた解の最初と最後（境界解）の混雑度を無限大に設定
            crowdingDistance[sortedIndicesByObjective[i][0]] = double.PositiveInfinity;
            crowdingDistance[sortedIndicesByObjective[i][size - 1]] = double.PositiveInfinity;
        }


        // 中間の解の混雑度を計算、sizeは配列のサイズ
        for (int i = 1; i < size - 1; i++)
        {
            double distance = 0.0;

            //目的関数ごとの混雑度の計算
            for (int j = 0; j < 3; j++)
            {
                double objectiveDiff = GetObjectiveValue(front[sortedIndicesByObjective[j][i + 1]], j) -
                                        GetObjectiveValue(front[sortedIndicesByObjective[j][i - 1]], j);
                distance += objectiveDiff; //目的関数ごとの距離
            }
            crowdingDistance[sortedIndicesByObjective[0][i]] = distance; //混雑度
        }

        return crowdingDistance;
    }




    // 指定された目的関数に基づいてフロントの解をソートする関数
    private static List<int> SortByObjective(List<Sample_Solution> front, int objectiveIndex)
    {
        List<int> indices = new List<int>(front.Count);
        for (int i = 0; i < front.Count; i++)
        {
            indices.Add(i);
        }

        //ラムダ式を使ったソート,(a, b) => a.CompareTo(b)で並べ替えることができる
        indices.Sort((x, y) => GetObjectiveValue(front[x], objectiveIndex).CompareTo(GetObjectiveValue(front[y], objectiveIndex)));

        return indices;
    }

    // 解と目的関数のインデックスを受け取り、その目的関数の値を返す関数
    private static double GetObjectiveValue(Sample_Solution solution, int objectiveIndex)
    {
        if (objectiveIndex < 0 || objectiveIndex >= solution.objectives.Length)
        {
            throw new ArgumentOutOfRangeException("Invalid objective index.");
        }

        return solution.objectives[objectiveIndex];
    }

    private static bool Dominates(Sample_Solution a, Sample_Solution b)
    {
        bool betterInAnyObjective = false;

        // 全ての目的関数（objectives）において非支配の関係を調べる
        bool isBetterOrEqualInAllObjectives = true;
        for (int i = 0; i < a.objectives.Length; i++)
        {
            if (a.objectives[i] > b.objectives[i])
            {
                isBetterOrEqualInAllObjectives = false;
                break;
            }
        }

        if (isBetterOrEqualInAllObjectives)
        {
            for (int i = 0; i < a.objectives.Length; i++)
            {
                if (a.objectives[i] < b.objectives[i])
                {
                    betterInAnyObjective = true;
                    break;
                }
            }
        }

        return betterInAnyObjective;
    }

}
