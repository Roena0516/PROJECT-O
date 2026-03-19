using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class NoteDataWrapper
{
    public List<NoteClass> notes; // 노트 데이터를 감싸는 클래스
    public SongInfoClass info;
}

public class SaveManager : MonoBehaviour
{
    public List<NoteClass> notes;

    public SongInfoClass info;

    public void SaveToJson(string filePath, float BPM, string artist, string title, string eventName, float level, string difficulty)
    {
        notes.Sort((note1, note2) => note1.beat.CompareTo(note2.beat));

        // NoteDataWrapper의 인스턴스를 생성하고 데이터 할당
        NoteDataWrapper wrapper = new NoteDataWrapper();
        wrapper.notes = notes;

        info.artist = artist;
        info.bpm = BPM;
        info.fileLocation = "asdf";
        info.title = title;
        info.eventName = eventName;
        info.level = level;
        info.difficulty = difficulty;
        wrapper.info = info;

        // 암호화 제거: .json 파일 직접 저장
        string json = JsonUtility.ToJson(wrapper, true); // prettyPrint를 true로 설정

        // 파일로 저장
        File.WriteAllText(filePath, json);
        Debug.Log("Chart saved to: " + filePath);
    }

}
