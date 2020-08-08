﻿#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.UI;

[ExecuteInEditMode]
public class BakeCNNWeights : EditorWindow {
    
    public TextAsset source;
    string SavePath1 = "Assets/SimpNet/Weights.asset";

    public int[,] savePos =
    {
        // Weights
        { 24, 384 },    // Kern1
        { 192, 256 },   // Kern2
        { 0, 0 },       // Kern3
        { 192, 0 },     // W1
        { 288, 256 },   // W2
        { 0, 384 },     // W3
        // Bias
        { 448, 0 },     // BK1
        { 449, 0 },     // BK2
        { 450, 0 },     // BK3
        { 451, 0 },     // BW1
        { 452, 0 },     // BW2
        { 453, 0 }      // BW3
    };

    [MenuItem("Tools/SCRN/Bake CNN Weights")]
    static void Init()
    {
        var window = GetWindowWithRect<BakeCNNWeights>(new Rect(0, 0, 400, 250));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Bake CNN Weights", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        source = (TextAsset) EditorGUILayout.ObjectField("CNN Weights (.txt):", source, typeof(TextAsset), false);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Bake!")) {
           
            if (source == null)
                ShowNotification(new GUIContent("Select the .txt output file"));
            else
                OnGenerateTexture();
        }
    }

    int ExtractFloats(Texture2D tex, Regex rgWs, int width, int wIndex)
    {
        Match mWs = rgWs.Match(source.text);
        string strWs = mWs.Groups[0].Value;

        if (strWs.Length == 0) {
            ShowNotification(new GUIContent("Wrong file format"));
            Debug.Log("Index: " + wIndex + " failed to capture correct values");
            return 0;
        }

        // Grab floats
        Regex rgFloats = new Regex("[-+]?[0-9]*\\.?[0-9]+([eE][-+]?[0-9]+)?");
        MatchCollection fvals = rgFloats.Matches(strWs);

        //Debug.Log("Count: " + fvals.Count);
        
        int c = 0;
        foreach (Match fValMatches in fvals)
        {
            int x = c % width;
            int y = c / width;

            CaptureCollection fValCapture = fValMatches.Captures;
            float stf = float.Parse(fValCapture[0].Value);
            tex.SetPixel(savePos[wIndex, 0] + x, savePos[wIndex, 1] + y,
                new Color(stf, 0.0f, 0.0f, 0.0f));
            c++;
        }

        return c;
    }

    void OnGenerateTexture()
    {
        Texture2D tex = new Texture2D(512, 512, TextureFormat.RFloat, false);
        
        // Kern1 weights
        Regex rgWs = new Regex("(?<=kern1:)[\\s\\S]*(?=bias1:)");
        ExtractFloats(tex, rgWs, 24, 0);

        // Kern2 weights
        rgWs = new Regex("(?<=kern2:)[\\s\\S]*(?=bias2:)");
        ExtractFloats(tex, rgWs, 96, 1);

        // Kern3 weights
        rgWs = new Regex("(?<=kern3:)[\\s\\S]*(?=bias3:)");
        ExtractFloats(tex, rgWs, 192, 2);

        // W1 weights
        rgWs = new Regex("(?<=w1:)[\\s\\S]*(?=biasw1:)");
        ExtractFloats(tex, rgWs, 256, 3);

        // W2 weights
        rgWs = new Regex("(?<=w2:)[\\s\\S]*(?=biasw2:)");
        ExtractFloats(tex, rgWs, 128, 4);

        // W3 weights
        rgWs = new Regex("(?<=w3:)[\\s\\S]*(?=biasw3:)");
        ExtractFloats(tex, rgWs, 24, 5);

        // bias1 weights
        rgWs = new Regex("(?<=bias1:)[\\s\\S]*(?=kern2:)");
        ExtractFloats(tex, rgWs, 1, 6);

        // bias2 weights
        rgWs = new Regex("(?<=bias2:)[\\s\\S]*(?=kern3:)");
        ExtractFloats(tex, rgWs, 1, 7);

        // bias3 weights
        rgWs = new Regex("(?<=bias3:)[\\s\\S]*?(?=w1:)");
        ExtractFloats(tex, rgWs, 1, 8);

        // biasw1 weights
        rgWs = new Regex("(?<=biasw1:)[\\s\\S]*?(?=w2:)");
        ExtractFloats(tex, rgWs, 1, 9);

        // biasw2 weights
        rgWs = new Regex("(?<=biasw2:)[\\s\\S]*?(?=w3:)");
        ExtractFloats(tex, rgWs, 1, 10);
        
        // biasw3 weights
        rgWs = new Regex("(?<=biasw3:)[\\s\\S]*");
        ExtractFloats(tex, rgWs, 1, 11);

        AssetDatabase.CreateAsset(tex, SavePath1);
        AssetDatabase.SaveAssets();

        ShowNotification(new GUIContent("Done"));
    }
}

#endif