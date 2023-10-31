using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;


//メインクラス
public class NSGA2_Run : MonoBehaviour
{
    public int populationSize = 100; //進化サイズ
    public int generations = 10; //世代数
    public float mutationProbability = 0.1f; //突然変異率
    public float crossoverProbability = 0.9f; //交叉率
    public int numberOfVariables = 12; // DTLZ2 for 3 objectives typically uses k=9 (12-3=9) decision variables
    public float lowerBound = 0f;
    public float upperBound = 1f;
    public float delay = 0.5f;

    public GameObject pointPrefab; // 小さな3Dオブジェクト（例：球）のプレハブ

    // 生成した視覚化点を一時保存するリスト
    private List<GameObject> instantiatedPoints = new List<GameObject>();

    //全体の初期集合
    public static List<Sample_Solution> population;
    //アーカイブ母集団
    public static List<Sample_Solution> archivePopulation = new List<Sample_Solution>(); // アーカイブ母集団を保持するリスト
    //子母集団
    public static List<Sample_Solution> offspringPopulation;
    //合体母集団
    List<Sample_Solution> combinedPopulation;

    void Start()
    {
        //NSGA2クラスのインスタンス化
        NSGAII nsga2 = new NSGAII(
            populationSize,
            generations,
            mutationProbability,
            crossoverProbability,
            numberOfVariables,
            lowerBound,
            upperBound);

        //NSGA2の進化(進化のみを行う）
        //nsga2.Evolve();


        //進化の視覚化（進化と視覚化をどちらも行う）
        StartCoroutine(RunAndVisualize());

        Debug.Log("完了でございます");

    }

    //結果の視覚化 //動作的にはEvolve関数と同じことをする
    public IEnumerator RunAndVisualize()
    {

        //全体の集合
        population = NSGAII.InitializePopulation();
        NSGAII.Evaluate(population);

        archivePopulation = population;
        //VisualizeParetoFront(NSGAII.archivePopulation);



        Debug.Log("今からループ回すべ");

        //進化開始
        for (int gen = 0; gen < NSGAII.generations; gen++)
        {
            
            // アーカイブ母集団を基にオフスプリングを生成
            offspringPopulation = NSGAII.NextGeneration(archivePopulation);

            // アーカイブ母集団とオフスプリングを結合
            combinedPopulation = archivePopulation.Concat(offspringPopulation).ToList();

            // 新しいアーカイブ母集団を抽出
            NSGAII.Evaluate(combinedPopulation);
            archivePopulation = NSGAII.ExtractNewArchive(combinedPopulation);

            // 現在の非支配解を視覚化
            VisualizeParetoFront(archivePopulation);
            //VisualizeParetoFront(combinedPopulation);

            // アーカイブ母集団を新しい母集団として使用
            population = archivePopulation;
            
            Debug.Log(gen + "世代目の計算完了");


            // 各世代の終わりに、少しの遅延を持たせる
            yield return new WaitForSeconds(delay);

        }
    }

  
    //アーカイブ母集団を表示
    public void VisualizeParetoFront(List<Sample_Solution> population)
    {

        // 以前に生成した全ての点を削除
        foreach (var point in instantiatedPoints)
        {
            Destroy(point);
        }
        instantiatedPoints.Clear();

        int count = 1;
        foreach (Sample_Solution sol in population)
        {

            
            if (sol.Rank == 1)  // ランクが1の場合のみ表示
            {
                //Debug.Log(count + ":アーカイブのランク" + sol.Rank);
                count++;

                Vector3 position = new Vector3((float)sol.objectives[0], (float)sol.objectives[1], (float)sol.objectives[2]);
                GameObject point = Instantiate(pointPrefab, position, Quaternion.identity);
                point.GetComponent<Renderer>().material.color = Color.red;
                instantiatedPoints.Add(point); // この点をリストに追加
            }
            else
            {
                Vector3 position = new Vector3((float)sol.objectives[0], (float)sol.objectives[1], (float)sol.objectives[2]);
                GameObject point = Instantiate(pointPrefab, position, Quaternion.identity);
                point.GetComponent<Renderer>().material.color = Color.blue;
                instantiatedPoints.Add(point); // この点をリストに追加
            }
        }

    }
}





