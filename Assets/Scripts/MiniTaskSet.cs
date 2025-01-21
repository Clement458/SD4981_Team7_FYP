using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Scriptable object that defines the trivia miniTasks.  Can be defined by a CSV file.
/// </summary>
[CreateAssetMenu()]
public class MiniTaskSet : ScriptableObject
{
    [SerializeField, Tooltip("Comma Separated Values that can be used to create the miniTasks.  Organized as \"MiniTask, Correct Answer, Other Choices\"")]
    private TextAsset _miniTaskCSV;

    [SerializeField, Tooltip("List of the trivia miniTasks.")]
    private List<MiniTask> _miniTasks;

    /// <summary>
    /// Simple getter for the MiniTasks
    /// </summary>
    public ReadOnlyCollection<MiniTask> MiniTasks => _miniTasks.AsReadOnly();

    [System.Serializable()]
    public class MiniTask
    {
        [Tooltip("The miniTask being asked.")]
        public string miniTask;

        [Tooltip("The correct answer to the miniTask.")]
        public string correctAnswer;
        public string[] decoyAnswers;
    }

    [ContextMenu("Create MiniTasks")]
    public void CreateMiniTasks()
    {
        _miniTasks = new List<MiniTask>();

        if (_miniTaskCSV == null)
        {
            Debug.LogWarning("No CSV Assigned.");
            return;
        }

        string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        char[] TRIM_CHARS = { '\"' };

        List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

        TextAsset data = _miniTaskCSV;

        var lines = Regex.Split(data.text, LINE_SPLIT_RE);

        if (lines.Length > 1)
        {
            var header = Regex.Split(lines[0], SPLIT_RE);
            for (var i = 1; i < lines.Length; i++)
            {

                var values = Regex.Split(lines[i], SPLIT_RE);
                if (values.Length == 0 || values[0] == "") continue;

                var entry = new Dictionary<string, object>();
                for (var j = 0; j < header.Length && j < values.Length; j++)
                {
                    string value = values[j];

                    value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                    value = value.Replace("\"\"", "\"");

                    object finalvalue = value;

                    entry[header[j]] = finalvalue;
                }
                list.Add(entry);
            }
        }

        // Goes through each line and sets up the miniTasks
        for (int i = 0; i < list.Count; i++)
        {
            MiniTask miniTask = new MiniTask();
            List<string> keys = new List<string>(list[i].Keys);

            miniTask.miniTask = list[i][keys[0]] as string;
            miniTask.correctAnswer = list[i][keys[1]] as string;

            miniTask.decoyAnswers = new string[3];
            miniTask.decoyAnswers[0] = list[i][keys[2]] as string;
            miniTask.decoyAnswers[1] = list[i][keys[3]] as string;
            miniTask.decoyAnswers[2] = list[i][keys[4]] as string;

            _miniTasks.Add(miniTask);
        }
    }
}
