using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileWriterScript
{
    private string foldername = "DebugLogs\\";
    public string writeDebugTxt(Landscape l, GraphRendererScript g)
    {
        string filename = "Debug_log_" + System.DateTime.Now.Millisecond + ".txt";
        this.writeDebugTxt(filename,l, g);
        return filename;
    }
    
    public void writeDebugTxt(string filename, Landscape l, GraphRendererScript g)
    {
        System.IO.StreamWriter writer = new System.IO.StreamWriter(foldername + filename, true);
        g.recalculateGraphProperties();
        l.sortAgents();
        writer.Write(g.toTxt() + "\n");
        writer.Write(l.toTxt() + "\n");
        writer.Close();
    }
}
