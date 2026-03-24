using UnityEngine;

public class LevelEditerNoteManager : MonoBehaviour
{
    public float ms;
    public NoteClass noteClass;
    public bool isInputed;

    private LevelEditer levelEditer;

    private void Start()
    {
        levelEditer = LevelEditer.Instance;
        levelEditer.OnNoteHit.AddListener(SetIsInputedToFalse);
    }

    private void Update()
    {
        if (levelEditer.currentMusicTime >= ms && !isInputed && levelEditer.isMusicPlaying)
        {
            isInputed = true;
            levelEditer.hitSoundInstance.start();
        }
    }

    private void SetIsInputedToFalse()
    {
        isInputed = false;

        // 롱노트의 추가 플래그도 초기화
        if (noteClass.type == "long")
        {
            noteClass.longNoteStarted = false;
            noteClass.isLongNotePressing = false;
            noteClass.pressedTime = 0f;
            noteClass.lastTickBeat = 0f;
            noteClass.startJudgement = "";
        }
    }

    // 노트 클릭 시 선택
    private void OnMouseDown()
    {
        if (levelEditer != null)
        {
            levelEditer.SelectNote(this);
        }
    }
}
