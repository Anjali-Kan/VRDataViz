using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class CSVParser{

    public string[] Headers { get; private set; }
    public List<string[]> Rows { get; private set; }
    public int RowCount => Rows.Count;
    public int ColumnCount => Headers.Length;

    public CSVParser(){
        Rows = new List<string[]>();
    }

    public bool Parse(string csvContent, char delimiter=','){
       
       try{

        string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            if (lines.Length < 2)
            {
                Debug.LogError("CSV must have header and at least one data row");
                return false;
            }

        // Parse header
        Headers = ParseLine(lines[0], delimiter);

        // Parse data rows
            Rows.Clear();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = ParseLine(lines[i], delimiter);
                
                // Handle missing values by padding with empty strings
                if (values.Length < Headers.Length)
                {
                    string[] padded = new string[Headers.Length];
                    for (int j = 0; j < Headers.Length; j++)
                    {
                        padded[j] = j < values.Length ? values[j] : "";
                    }
                    values = padded;
                }
                
                Rows.Add(values);
            }



        Debug.Log($"Parsed CSV: {ColumnCount} columns, {RowCount} rows");
        return true;

       }catch(Exception e){
           Debug.LogError($"Error parsing CSV: {e.Message}");
           return false;
       }
    }

    private string[] ParseLine(string line, char delimiter){
        List<string> fields = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for(int i=0; i<line.Length; i++){
            char c = line[i];

            if(c == '"'){
                //Escaped quotes
                if(inQuotes && i + 1 < line.Length && line[i + 1] == '"'){
                    currentField += '"';
                    i++; //Skip next quote
                }else{
                    inQuotes = !inQuotes;
                }
            }else if(c == delimiter && !inQuotes){
                fields.Add(currentField.Trim());
                currentField = "";
            }else{
                currentField += c;
            }
        }
        fields.Add(currentField.Trim());
        return fields.ToArray();
    }

    public float[] GetColumnAsFloat(int columnIndex)
    {
        float[] values = new float[RowCount];
        
        for (int i = 0; i < RowCount; i++)
        {
            if (float.TryParse(Rows[i][columnIndex], out float value))
            {
                values[i] = value;
            }
            else
            {
                values[i] = 0f; // Default for non-numeric
            }
        }
        
        return values;
    }

    public string[] GetColumnAsString(int columnIndex)
    {
        string[] values = new string[RowCount];
        
        for (int i = 0; i < RowCount; i++)
        {
            values[i] = Rows[i][columnIndex];
        }
        
        return values;
    }

    public bool IsColumnNumeric(int columnIndex)
    {
        int numericCount = 0;
        int sampleSize = Mathf.Min(10, RowCount);
        
        for (int i = 0; i < sampleSize; i++)
        {
            if (float.TryParse(Rows[i][columnIndex], out _))
            {
                numericCount++;
            }
        }
        
        return numericCount > sampleSize * 0.8f;
    }

    public int GetColumnIndex(string headerName)
    {
        for (int i = 0; i < Headers.Length; i++)
        {
            if (Headers[i].Equals(headerName, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return -1;
    }




}