using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TextFileHandler
{
    private string _path = $"{Application.dataPath}/Log.txt";

    public TextFileHandler(string fileName)
    {
        SetPath(fileName);
    }

    void SetPath(string fileName)
    {
        _path = $"{Application.dataPath}/{fileName}.txt";
    }

    public (bool,string) GetFileText()
    {
        if (System.IO.File.Exists(_path))
        {
            return (true, File.ReadAllText(_path));
        }
        return (false,"Error");
    }

    public void AddTextToFile(string addText)
    {
        if (!File.Exists(_path))
        {
            File.WriteAllText(_path,$"{addText}\r\n");
        }
        else
        {
            //think i will always overwrite the data
            File.WriteAllText(_path,$"{addText}\r\n");
            // File.AppendAllText(_path,$"{addText}\r\n");
        }
    }
}
