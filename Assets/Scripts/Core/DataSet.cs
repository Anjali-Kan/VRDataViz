using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ColumnInfo
{
    public string Name;
    public int Index;
    public bool IsNumeric;
    public float Min;
    public float Max;
    public string[] UniqueCategories;
}

public class DataSet
{
    public CSVParser Parser { get; private set; }
    public List<ColumnInfo> Columns { get; private set; }
    public bool IsLoaded => Parser != null && Parser.RowCount > 0;
    public DataSet()
    {
        Columns = new List<ColumnInfo>();
    }

    public bool LoadFromCSV(string csvContent, char delimiter=',')
    {
        Parser = new CSVParser();
        if(!Parser.Parse(csvContent, delimiter))
            return false;   
    

        AnalyzaColumns();
        return true;

    }

    private void AnalyzaColumns(){
        Columns.Clear();
        for(int i =0; i < Parser.ColumnCount; i++){

            ColumnInfo info = new ColumnInfo
            {
                Name = Parser.Headers[i],
                Index = i,
                IsNumeric = Parser.IsColumnNumeric(i)
            };

            if (info.IsNumeric)
            {
                float[] values = Parser.GetColumnAsFloat(i);
                info.Min = float.MaxValue;
                info.Max = float.MinValue;
                
                foreach (float v in values)
                {
                    if (v < info.Min) info.Min = v;
                    if (v > info.Max) info.Max = v;
                }
            }else
            {
                // Find unique categories
                HashSet<string> unique = new HashSet<string>();
                string[] values = Parser.GetColumnAsString(i);
                
                foreach (string v in values)
                {
                    unique.Add(v);
                }
                
                info.UniqueCategories = new string[unique.Count];
                unique.CopyTo(info.UniqueCategories);
            }
            Columns.Add(info);
        }
        Debug.Log($"Analyzed {Columns.Count} columns");
    }


    public float[] GetNormalizedColumn(int columnIndex, float targetMin = 0f, float targetMax = 1f)
    {
        ColumnInfo info = Columns[columnIndex];
        
        if (!info.IsNumeric)
        {
            Debug.LogWarning($"Column {info.Name} is not numeric, returning zeros");
            return new float[Parser.RowCount];
        }
        
        float[] raw = Parser.GetColumnAsFloat(columnIndex);
        float[] normalized = new float[raw.Length];
        
        float range = info.Max - info.Min;
        if (range == 0) range = 1; // Avoid divide by zero
        
        for (int i = 0; i < raw.Length; i++)
        {
            float t = (raw[i] - info.Min) / range; // 0 to 1
            normalized[i] = Mathf.Lerp(targetMin, targetMax, t);
        }
        
        return normalized;
    }

    public int GetCategoryIndex(int columnIndex, int rowIndex)
    {
        ColumnInfo info = Columns[columnIndex];
        
        if (info.IsNumeric || info.UniqueCategories == null)
        {
            return 0;
        }
        
        string value = Parser.Rows[rowIndex][columnIndex];
        
        for (int i = 0; i < info.UniqueCategories.Length; i++)
        {
            if (info.UniqueCategories[i] == value)
            {
                return i;
            }
        }
        
        return 0;
    }

    public ColumnInfo GetColumnByName(string name)
    {
        foreach (ColumnInfo col in Columns)
        {
            if (col.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return col;
            }
        }
        return null;
    }

     public string[] GetNumericColumnNames()
    {
        List<string> names = new List<string>();
        
        foreach (ColumnInfo col in Columns)
        {
            if (col.IsNumeric)
            {
                names.Add(col.Name);
            }
        }
        
        return names.ToArray();
    }

    public string[] GetAllColumnNames()
    {
        List<string> names = new List<string>();
        
        foreach (ColumnInfo col in Columns)
        {
            names.Add(col.Name);
        }
        
        return names.ToArray();
    }

}