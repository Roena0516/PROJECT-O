using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Note : MonoBehaviour
{
    private float speed;

    public float BPM;

    public bool isSet;
    public double ms;

    public bool isEndNote;
    public bool isInputed;

    private LineInputChecker line;
    private JudgementManager judgement;
    private NoteGenerator noteGenerator;

    public NoteClass noteClass;

    private float startY = 87f;
    private float endY = -96f;

    private float YPosition = 0f;

    private double dropStartTime;

    private Coroutine moveNoteRoutine;


    void Start()
    {
        isSet = false;
        isEndNote = false;
        isInputed = false;

        line = LineInputChecker.Instance;
        judgement = JudgementManager.Instance;
        noteGenerator = NoteGenerator.Instance;

        speed = noteGenerator.speed;
        dropStartTime = (ms - noteGenerator.fallTime) / 1000f;
        YPosition = noteClass.type == "hold" ? 2f : 0.001f;

        if (noteClass.type == "hold")
        {
            gameObject.transform.localScale = new Vector3(7f * noteClass.width, 1f, 1f);
        }

        float oneBeatDuration = 60f / BPM * 1000f;


        if (noteClass.type == "null")
        {
            moveNoteRoutine = StartCoroutine(MoveLongNote());
        }
        else
        {
            moveNoteRoutine = StartCoroutine(MoveNote());
        }
    }

    public void SetNote()
    {
        dropStartTime = line.currentTime;
        speed = noteGenerator.speed;
        isSet = true;
    }

    private void OnDestroy()
    {
        StopCoroutine(moveNoteRoutine);
    }

    public IEnumerator MoveNote()
    {
        while (true)
        {
            dropStartTime = (ms - noteGenerator.fallTime) / 1000f;
            double elapsedTime = line.currentTime - dropStartTime;
            float progress = (float)(elapsedTime * speed / (startY - endY));
            progress = Mathf.Clamp01(progress);  // 0 ~ 1 사이로 제한
            float currentY = Mathf.Lerp(startY, endY, progress);
            transform.position = new Vector3(transform.position.x, YPosition, currentY);

            yield return null;
        }
    }

    public IEnumerator MoveLongNote()
    {
        float originScaleZ = gameObject.transform.localScale.z;
        double longNoteEndTimeMs = ms + (60000f / noteGenerator.BPM * noteClass.length);
        double remainingDistance = originScaleZ;

        endY = 10f;

        while (longNoteEndTimeMs - (line.currentTime * 1000f) >= -200f)
        {
            double currentTimeMs = line.currentTime * 1000f;
            double duration = longNoteEndTimeMs - currentTimeMs;
            remainingDistance = speed * (duration / 1000f);

            if (line.currentTime * 1000f <= ms)
            {
                remainingDistance = originScaleZ;
            }

            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, (float)remainingDistance);


            dropStartTime = (ms - noteGenerator.fallTime) / 1000f;
            double elapsedTime = line.currentTime - dropStartTime;
            float progress = (float)(elapsedTime * speed / (startY - endY));
            progress = Mathf.Clamp01(progress);  // 0 ~ 1 사이로 제한
            float currentY = Mathf.Lerp(startY, endY, progress);

            currentY += (float)remainingDistance / 2f;
            transform.position = new Vector3(transform.position.x, YPosition, currentY);

            yield return null;
        }

        Destroy(gameObject);
        yield break;
    }

    private void Misser()
    {
        // 롱노트와 longNote(null)는 JudgementManager에서 처리하므로 여기서 제외
        if (noteClass.type == "long" || noteClass.type == "null")
            return;

        if (!noteClass.isInputed && (line.currentTime * 1000f) - ms >= 200f)
        {
            judgement.PerformAction(noteClass, "Miss", ms);
            judgement.ClearCombo();
            isSet = false;
        }
    }

    private void BellPerformer()
    {
        if (noteClass.type == "hold" && (noteClass.ms - (line.currentTime * 1000f) <= 0 && noteClass.ms - (line.currentTime * 1000f) >= -160))
        {
            if (Math.Abs(judgement.tsumabuki.transform.position.x - gameObject.transform.position.x) <= 3.5f + (1.75f * noteClass.width) + 2.25f)
            {
                line.judgementManager.PerformAction(noteClass, "PerfectP", noteClass.ms);
                line.judgementManager.AddCombo(1);
            }
        }
    }

    private void AutoPlayPerformer()
    {
        if (line.isAutoPlay && !noteClass.isInputed && (noteClass.ms - (line.currentTime * 1000f) <= 0))
        {
            line.judgementManager.PerformAction(noteClass, "PerfectP", noteClass.ms);
            line.judgementManager.AddCombo(1);

            Debug.Log($"AutoPlay note.ms: {noteClass.ms}, currentTime: {line.currentTime * 1000f}");
        }
    }

    void Update()
    {
        speed = noteGenerator.speed;

        Misser();

        BellPerformer();

        AutoPlayPerformer();
    }
}
