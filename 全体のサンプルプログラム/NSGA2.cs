using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class NSGAII
{
    // ポピュレーションの設定値
    public static int populationSize;
    public static int generations;
    public static float mutationProbability;
    public static float crossoverProbability;
    public static int numberOfVariables;
    public static float lowerBound;
    public static float upperBound;

    //public static List<Sample_Solution> archivePopulation = new List<Sample_Solution>(); // アーカイブ母集団を保持するリスト


    // コンストラクタ：初期化と設定
    public NSGAII(int _populationSize, int _generations, float _mutationProbability, float _crossoverProbability, int _numberOfVariables, float _lowerBound, float _upperBound)
    {
        populationSize = _populationSize;
        generations = _generations;
        mutationProbability = _mutationProbability;
        crossoverProbability = _crossoverProbability;
        numberOfVariables = _numberOfVariables;
        lowerBound = _lowerBound;
        upperBound = _upperBound;
    }

    // 進化の主要なルーチン
    public void Evolve()
    {
        List<Sample_Solution> population = InitializePopulation(); // 初期のポピュレーションを生成
        Evaluate(population); // ポピュレーションの評価                        
        NSGA2_Run.archivePopulation = ExtractNewArchive(population); // 初期のアーカイブ母集団を初期化

        //進化開始
        for (int i = 0; i < generations; i++)
        {

            List<Sample_Solution> offspringPopulation = NextGeneration(population); //オフスプリングは次の世代の子母集団
            List<Sample_Solution> combinedPopulation = NSGA2_Run.archivePopulation.Concat(offspringPopulation).ToList(); // アーカイブ母集団と小母集団を結合

            //全母集団を非優劣ソート、混雑度ソートによって選別し、アーカイブ母集団を作成する
            List<Sample_Solution> newArchive = ExtractNewArchive(combinedPopulation);

            //アーカイブ母集団の完成
            NSGA2_Run.archivePopulation = newArchive;

            //子母集団の生成
            population = NSGA2_Run.archivePopulation;



        }
    }

    // 初期のポピュレーションを生成
    public static List<Sample_Solution> InitializePopulation()
    {
        List<Sample_Solution>population2 = new List<Sample_Solution>();

        for (int i = 0; i < populationSize; i++)
        {
            Sample_Solution sol = new Sample_Solution(numberOfVariables);
            for (int j = 0; j < numberOfVariables; j++)
            {
                sol.variables[j] = UnityEngine.Random.Range(lowerBound, upperBound); // 変数をランダムに初期化
            }
            population2.Add(sol);
        }
        return population2;
    }

    // ポピュレーションの評価
    public static void Evaluate(List<Sample_Solution> population)
    {
        foreach (Sample_Solution sol in population)
        {
            sol.objectives = DTLZ2(sol.variables); // DTLZ2関数を用いた評価
        }
    }


    // DTLZ2関数の実装
    public static float[] DTLZ2(float[] x)
    {
        int M = 3;  // 目的関数の数
        int k = x.Length - M + 1;  // k parameter (usually k=10 for DTLZ2)

        float g = 0.0f;

        // gの計算 (最後のk変数に基づく)
        for (int i = 1; i < M; i++)
        {
            g += (x[i] - 0.5f) * (x[i] - 0.5f);
        }

        //for (int i = M; i < M + k; i++)
        //{
        //    g += (x[i - 1] - 0.5f) * (x[i - 1] - 0.5f);  // i-1 because array index starts from 0
        //}

        float[] objectives = new float[M];

        // 目的関数 f1
        objectives[0] = (1.0f + g) * Mathf.Cos(x[0] * Mathf.PI / 2) * Mathf.Cos(x[1] * Mathf.PI / 2);

        // 目的関数 f2
        objectives[1] = (1.0f + g) * Mathf.Cos(x[0] * Mathf.PI / 2) * Mathf.Sin(x[1] * Mathf.PI / 2);

        // 目的関数 f3
        objectives[2] = (1.0f + g) * Mathf.Sin(x[0] * Mathf.PI / 2);

        return objectives;


        /*
        int M = x.Length;
        float g = 0;

        for (int i = 1; i < M; i++)
        {
            g += (x[i] - 0.5f) * (x[i] - 0.5f);
        }

        float[] objectives = new float[3];
        objectives[0] = (1 + g) * Mathf.Cos(x[0] * Mathf.PI / 2);
        objectives[1] = (1 + g) * Mathf.Sin(x[0] * Mathf.PI / 2);
        objectives[2] = (1 + g);

        return objectives;
        */
    }


    // 次の世代のポピュレーションを生成
    public static List<Sample_Solution> NextGeneration(List<Sample_Solution> population)
    {
        //次の世代のPopulationを保持するやつ
        List<Sample_Solution> offspringPopulationNext = new List<Sample_Solution>();

        //人口のサイズになるまで
        while (offspringPopulationNext.Count < populationSize)
        {
            // 1. トーナメント選択を用いてペアの親を選択
            Sample_Solution parent1 = TournamentSelect(population);
            Sample_Solution parent2 = TournamentSelect(population);

            // 2. 交叉を用いて2つの子供を生成
            if (UnityEngine.Random.value < crossoverProbability) //交叉するかしないか
            {
                List<Sample_Solution> children = Crossover(parent1, parent2);
                offspringPopulationNext.AddRange(children);
            }
            else
            {
                offspringPopulationNext.Add(parent1);
                offspringPopulationNext.Add(parent2);
            }


            // 3.すべての子に対して変異率に応じて突然変異を適用して子供の遺伝子を変更
            foreach (var child in offspringPopulationNext)
            {
                Mutate(child);
            }

        }

        return offspringPopulationNext;
    }



    //トーナメント選択　５個を選択し、それからも最も良いものを選ぶもの選ぶ
    private static Sample_Solution TournamentSelect(List<Sample_Solution> population)
    {
        int tournamentSize = 5; // トーナメントのサイズ
        List<Sample_Solution> tournament = new List<Sample_Solution>();

        for (int i = 0; i < tournamentSize; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, population.Count);
            tournament.Add(population[randomIndex]);
        }

        // 最良の解を返す（ここでは目的関数の合計値が最も低いものを最良とします）
        return tournament.OrderBy(sol => sol.objectives.Sum()).FirstOrDefault();
    }



    ///交叉
    private static List<Sample_Solution> Crossover(Sample_Solution parent1, Sample_Solution parent2)
    {
        // ここでは単純な1点交叉を使用します
        Sample_Solution child1 = new Sample_Solution(numberOfVariables);
        Sample_Solution child2 = new Sample_Solution(numberOfVariables);

        int crossoverPoint = UnityEngine.Random.Range(0, numberOfVariables);

        for (int i = 0; i < numberOfVariables; i++)
        {
            if (i < crossoverPoint)
            {
                child1.variables[i] = parent1.variables[i];
                child2.variables[i] = parent2.variables[i];
            }
            else
            {
                child1.variables[i] = parent2.variables[i];
                child2.variables[i] = parent1.variables[i];
            }
        }

        return new List<Sample_Solution>() { child1, child2 };
    }


    //突然変異
    private static void Mutate(Sample_Solution solution)
    {
        for (int i = 0; i < numberOfVariables; i++)
        {
            if (UnityEngine.Random.value < mutationProbability)
            {
                // ここでは単純な突然変異を使用し、変数の値をランダムに変更します
                solution.variables[i] = UnityEngine.Random.Range(lowerBound, upperBound);
            }
        }
    }








    // 非支配ソリューションを抽出
    public static List<Sample_Solution> ExtractNewArchive(List<Sample_Solution> combinedPopulation)
    {
        // 1. 非支配ソートを実行して、各解のランクを取得
        List<int> ranks = NonDominatedSorting.Sort(combinedPopulation);

        // 2. 最高ランクから順に、アーカイブ母集団を構築
        
        int maxRank = ranks.Max();

        //新しいアーカイブ母集団
        List<Sample_Solution> newArchivePopulation = new List<Sample_Solution>();
        for (int rank = 1; rank <= maxRank && newArchivePopulation.Count < populationSize; rank++)
        {
            List<Sample_Solution> currentFront = new List<Sample_Solution>();
            for (int i = 0; i < combinedPopulation.Count; i++)
            {
                if (ranks[i] == rank)
                {
                    currentFront.Add(combinedPopulation[i]);
                }
            }

            // 3. 混雑度を計算
            List<double> crowdingDistances = NonDominatedSorting.CalculateCrowdingDistance(currentFront);

            // 4. アーカイブ母集団に解を追加
            if (newArchivePopulation.Count + currentFront.Count <= populationSize)
            {
                newArchivePopulation.AddRange(currentFront);
            }
            else
            {
                // 混雑度が高い解を優先して追加
                List<int> sortedIndices = crowdingDistances
                    .Select((value, index) => new { Value = value, Index = index })
                    .OrderByDescending(item => item.Value)
                    .Select(item => item.Index)
                    .ToList();

                for (int i = 0; i < sortedIndices.Count && newArchivePopulation.Count < populationSize; i++)
                {
                    newArchivePopulation.Add(currentFront[sortedIndices[i]]);
                }
            }
        }

        // newArchivePopulation のランクを再計算
        List<int> newArchiveRanks = NonDominatedSorting.Sort(newArchivePopulation);
        for (int i = 0; i < newArchivePopulation.Count; i++)
        {
            newArchivePopulation[i].Rank = newArchiveRanks[i];
        }
    

        return newArchivePopulation;
    }




}
