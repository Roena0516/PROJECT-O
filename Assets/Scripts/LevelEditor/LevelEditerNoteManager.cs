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
