using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// csv 받아오는 함수
public class CSVReader
{
	static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
	static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
	static char[] TRIM_CHARS = { '\"' };
	
    /// <summary>
    /// 1. csv 받아와서 줄마다 나누기
    /// 2. 첫 줄 항목의 이름 받아오기
    /// 3. 세번째 줄부터 데이터 값 변환해서 딕셔너리에 추가하기
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
	public static List<Dictionary<string, object>> Read(string file)
	{

		var list = new List<Dictionary<string, object>>();
		TextAsset data = Resources.Load (file) as TextAsset;
        // 1.
		var lines = Regex.Split (data.text, LINE_SPLIT_RE);
		if(lines.Length <= 1)
            return list;

        // 2.
        var header = Regex.Split(lines[0], SPLIT_RE);
        // 3.
		for(var i = 2; i < lines.Length; i++)
        {
			var values = Regex.Split(lines[i], SPLIT_RE);
			if(values.Length == 0 ||values[0] == "")
                continue;
			
			var entry = new Dictionary<string, object>();
			for(var j=0; j < header.Length && j < values.Length; j++ )
            {
				string value = values[j];
                //Debug.Log(value);
				value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

				object finalvalue = value;
				int n;
				float f;
				if(int.TryParse(value, out n))
                {
					finalvalue = n;
				}
                else if (float.TryParse(value, out f))
                {
					finalvalue = f;
				}
				entry[header[j]] = finalvalue;
			}
			list.Add (entry);
		}
		return list;
	}
}
