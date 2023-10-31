using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample_Solution
{


    //非優劣ソートにおけるランク
    public int Rank { get; set; } 

    // numberOfVariablesに基づいて変数のサイズを初期化するための新しいプロパティとコンストラクタ
    public float[] variables;
    public float[] objectives;

    public Sample_Solution(int numberOfVars)
    {
        variables = new float[numberOfVars];
        objectives = new float[3]; // 3つの目的関数を持っていると仮定しています
    }

    // 他の追加の属性やメソッドが必要な場合はこちらに追加してください。
}