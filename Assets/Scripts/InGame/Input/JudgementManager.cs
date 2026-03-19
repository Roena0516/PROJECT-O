using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class JudgementManager : MonoBehaviour
{
    private float perfectp = 25f;
    private float perfect = 50f;
    private float great = 70f;
    private float good = 110f;
    private float bad = 160f;

    public int combo;
    public float rate;

    public NoteGenerator noteGenerator;
    public GameManager gameManager;
    public LineInputChecker lineInputChecker;
    // public SyncRoomManager syncRoomManager; // 클래스가 존재하지 않아 제거됨
    private SettingsManager settings;
    public ParticleManager particle;
    [SerializeField] private UIManager UIManager;
    [SerializeField] private Animator _unityAnimator;
    [SerializeField] private InGameAnimation _animator;

    [SerializeField] private GameObject _FCAPFolder;

    public GameObject tsumabuki;

    public bool isAP;
    public bool isFC;

    public Dictionary<string, float> noteTypeRate = new Dictionary<string, float>();
    public Dictionary<string, int> judgeCount = new Dictionary<string, int>();

    public static JudgementManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        settings = SettingsManager.Instance;

        rate = 100f;

        isAP = false;
        isFC = false;

        noteTypeRate["normal"] = 0f;
        noteTypeRate["hold"] = 0f;
        noteTypeRate["long"] = 0f;
        noteTypeRate["null"] = 0f;

        judgeCount["PerfectP"] = 0;
        judgeCount["Perfect"] = 0;
        judgeCount["Great"] = 0;
        judgeCount["Good"] = 0;
        judgeCount["Bad"] = 0;
        judgeCount["Miss"] = 0;

        ClearCombo();
        UIManager.UpdateJudgeCountText(judgeCount);
        UIManager.ChangeRate(rate);
    }

    public void CalcRate()
    {
        float rateAllNote = (noteGenerator.noteTypeCounts["normal"] * 1) + (noteGenerator.noteTypeCounts["hold"] * 1) + (noteGenerator.noteTypeCounts["long"] * 2);

        noteTypeRate["normal"] = (noteGenerator.noteTypeCounts["normal"] > 0) ? (noteGenerator.noteTypeCounts["normal"] / rateAllNote * 100 / noteGenerator.noteTypeCounts["normal"]) : 0;
        noteTypeRate["hold"] = (noteGenerator.noteTypeCounts["hold"] > 0) ? (noteGenerator.noteTypeCounts["hold"] / rateAllNote * 100) / noteGenerator.noteTypeCounts["hold"] : 0;
        noteTypeRate["long"] = (noteGenerator.noteTypeCounts["long"] > 0) ? (noteGenerator.noteTypeCounts["long"] * 2 / rateAllNote * 100) / noteGenerator.noteTypeCounts["long"] : 0;
    }

    public void Judge(int raneNumber, double currentTimeMs)
    {
        var filteredNotes = noteGenerator.notes
        .Where(note => Mathf.Abs((float)(note.ms - currentTimeMs)) <= 161)
        .ToList();

        foreach (NoteClass note in filteredNotes)
        {
            if (note.type == "up" && raneNumber + 1 == note.position && !note.isInputed)
            {
                break;
            }

            float timeDifference = Mathf.Abs((float)(note.ms - currentTimeMs));

            if (timeDifference <= bad && note.type == "hold" && raneNumber + 1 == note.position && !note.isInputed)
            {
                SetHoldInputed(note);
                break;
            }
            if (timeDifference <= perfectp && note.type == "normal" && raneNumber + 1 == note.position && !note.isInputed)
            {
                PerformAction(note, "PerfectP", currentTimeMs);
                AddCombo(1);
                break;
            }
            if (timeDifference <= perfect && note.type == "normal" && raneNumber + 1 == note.position && !note.isInputed)
            {
                PerformAction(note, "Perfect", currentTimeMs);
                AddCombo(1);
                break;
            }
            if (timeDifference <= great && note.type == "normal" && raneNumber + 1 == note.position && !note.isInputed)
            {
                PerformAction(note, "Great", currentTimeMs);
                AddCombo(1);
                break;
            }
            if (timeDifference <= good && note.type == "normal" && raneNumber + 1 == note.position && !note.isInputed)
            {
                PerformAction(note, "Good", currentTimeMs);
                AddCombo(1);
                break;
            }
            if (timeDifference <= bad && note.type == "normal" && raneNumber + 1 == note.position && !note.isInputed)
            {
                PerformAction(note, "Bad", currentTimeMs);
                ClearCombo();
                break;
            }

            // 롱노트 시작 판정 (일반 노트와 동일)
            if (note.type == "long" && raneNumber + 1 == note.position && !note.longNoteStarted)
            {
                double Ms = note.ms - currentTimeMs;

                // pressedTime 초기화 (총 시간으로 시작)
                float totalTime = 60000f / noteGenerator.BPM * note.length;
                note.pressedTime = totalTime;

                if (timeDifference <= perfectp)
                {
                    note.startJudgement = "PerfectP";
                    note.longNoteStarted = true;
                    note.isLongNotePressing = true;
                    Destroy(note.noteObject); // 첫 노트 제거
                    StartCoroutine(UIManager.JudgementTextShower("PerfectP", Ms, note.position));
                    AddCombo(1);
                    break;
                }
                else if (timeDifference <= perfect)
                {
                    note.startJudgement = "Perfect";
                    note.longNoteStarted = true;
                    note.isLongNotePressing = true;
                    Destroy(note.noteObject); // 첫 노트 제거
                    StartCoroutine(UIManager.JudgementTextShower("Perfect", Ms, note.position));
                    AddCombo(1);
                    break;
                }
                else if (timeDifference <= great)
                {
                    note.startJudgement = "Great";
                    note.longNoteStarted = true;
                    note.isLongNotePressing = true;
                    Destroy(note.noteObject); // 첫 노트 제거
                    StartCoroutine(UIManager.JudgementTextShower("Great", Ms, note.position));
                    AddCombo(1);
                    break;
                }
                else if (timeDifference <= good)
                {
                    note.startJudgement = "Good";
                    note.longNoteStarted = true;
                    note.isLongNotePressing = true;
                    Destroy(note.noteObject); // 첫 노트 제거
                    StartCoroutine(UIManager.JudgementTextShower("Good", Ms, note.position));
                    AddCombo(1);
                    break;
                }
                else if (timeDifference <= bad)
                {
                    note.startJudgement = "Bad";
                    note.longNoteStarted = true;
                    note.isLongNotePressing = true;
                    Destroy(note.noteObject); // 첫 노트 제거
                    StartCoroutine(UIManager.JudgementTextShower("Bad", Ms, note.position));
                    ClearCombo();
                    break;
                }
            }
        }
    }

    public void SetHoldInputed(NoteClass note)
    {
        note.isInputed = true;
    }

    public void UpJudge(int raneNumber, double currentTimeMs)
    {
        var filteredNotes = noteGenerator.notes
        .Where(note => Mathf.Abs((float)(note.ms - currentTimeMs)) <= 1000)
        .ToList();

        foreach (NoteClass note in filteredNotes)
        {
            float timeDifference = Mathf.Abs((float)(note.ms - currentTimeMs));
            double notAbsDiff = note.ms - currentTimeMs;

            if ((note.ms - (currentTimeMs) <= 50 && note.ms - (currentTimeMs) > 0) && note.type == "hold" && raneNumber + 1 == note.position && !note.isInputed)
            {
                note.isInputed = true;
                break;
            }

            if (notAbsDiff >= -80 && notAbsDiff < 60 && note.type == "up" && raneNumber + 1 == note.position && !note.isInputed)
            {
                PerformAction(note, "PerfectP", currentTimeMs);
                AddCombo(1);
                break;
            }
            if (notAbsDiff >= -200 && notAbsDiff < -80 && note.type == "up" && raneNumber + 1 == note.position && !note.isInputed)
            {
                PerformAction(note, "Great", currentTimeMs);
                AddCombo(1);
                break;
            }
            if (notAbsDiff > 60 && notAbsDiff <= 130 && note.type == "up" && raneNumber + 1 == note.position && !note.isInputed)
            {
                PerformAction(note, "Great", currentTimeMs);
                AddCombo(1);
                break;
            }
        }
    }

    public void AddCombo(int amount)
    {
        combo += amount;
        UIManager.SetCombo(combo);
    }

    public void ClearCombo()
    {
        combo = 0;
        UIManager.ClearCombo();
    }

    public void PerformAction(NoteClass note, string judgement, double currentTimeMs)
    {
        Debug.Log($"{judgement}: {note.ms}, input: {currentTimeMs} type: {note.type}");
        note.isInputed = true;
        Destroy(note.noteObject);

        if (!noteTypeRate.ContainsKey(note.type))
        {
            Debug.LogError($"Unknown note type: '{note.type}' (length: {note.type.Length})");
            return;
        }

        Debug.Log(noteTypeRate[note.type]);

        if (judgement == "Great")
        {
            ChangeRate(noteTypeRate[note.type], 0.25f);
        }
        if (judgement == "Good")
        {
            ChangeRate(noteTypeRate[note.type], 0.5f);
        }
        if (judgement == "Bad")
        {
            ChangeRate(noteTypeRate[note.type], 1f);
        }
        if (judgement == "Miss")
        {
            ChangeRate(noteTypeRate[note.type], 1f);
        }
        double Ms = note.ms - currentTimeMs;
        judgeCount[judgement]++;
        UIManager.UpdateJudgeCountText(judgeCount);

        if (judgement != "Miss")
        {
            // _animator.SpawnKeyBombEffect(note.position - 1);
            // SFXLoader.Instance.PlaySFX("hitsound_tamb.wav"); // SFXLoader 클래스가 존재하지 않아 제거됨
        }

        if (note.isEndNote == true)
        {
            if (judgeCount["Miss"] == 0 && judgeCount["Bad"] == 0)
            {
                //UIManager.SetFCAPText("FULL COMBO");
                _FCAPFolder.SetActive(true);
                _unityAnimator.Play("New Animation");
                isFC = true;
            }
            if (judgeCount["Miss"] == 0 && judgeCount["Bad"] == 0 && judgeCount["Good"] == 0 && judgeCount["Great"] == 0)
            {
                UIManager.SetFCAPText("ALL PERFECT");
                isAP = true;
            }

            Debug.Log($"{note.ms}, {note.type}, {note.position}, {note.isEndNote}, {note.beat}");

            gameManager.isLevelEnd = true;
        }
        StartCoroutine(UIManager.JudgementTextShower(judgement, Ms, note.position));

        //if (judgement != "Miss")
        //{
        //    particle.EmitParticle(note.position - 1);
        //}

        // SyncRoom 관련 코드 (SyncRoomManager 클래스가 존재하지 않아 제거됨)
        // if (gameManager.isSyncRoom && judgement != "Miss")
        // {
        //     syncRoomManager.inputConut++;
        //     syncRoomManager.msCount += (int)Ms;
        //     syncRoomManager.CalcAvg();
        // }
    }

    private void ChangeRate(float typeRate, float ratio)
    {
        rate -= typeRate * ratio;
        UIManager.ChangeRate(rate);
    }

    private void Update()
    {
        if (noteGenerator == null || noteGenerator.notes == null || lineInputChecker == null)
            return;

        double currentTimeMs = lineInputChecker.currentTime * 1000f;

        foreach (NoteClass note in noteGenerator.notes)
        {
            if (note.type == "long" && note.longNoteStarted && !note.isInputed)
            {
                float longNoteEndTimeMs = note.ms + (60000f / noteGenerator.BPM * note.length);

                // 롱노트 진행 중
                if (currentTimeMs >= note.ms && currentTimeMs <= longNoteEndTimeMs)
                {
                    int laneIndex = (int)note.position - 1;

                    // 키가 안눌려있는지 확인
                    if (!lineInputChecker.isHolding[laneIndex])
                    {
                        note.isLongNotePressing = false;
                        note.pressedTime -= Time.deltaTime * 1000f; // 누적 시간 감소 (ms)
                    }
                    else
                    {
                        note.isLongNotePressing = true;
                    }

                    // tick마다 현재 판정 표시
                    if (note.tick > 0)
                    {
                        float oneBeatDuration = 60000f / noteGenerator.BPM; // ms
                        float currentBeat = note.beat + ((float)(currentTimeMs - note.ms) / oneBeatDuration);

                        if (currentBeat - note.lastTickBeat >= note.tick)
                        {
                            ShowLongNoteTickJudgement(note);
                            note.lastTickBeat = currentBeat;
                        }
                    }
                }
                // 롱노트 종료
                else if (currentTimeMs > longNoteEndTimeMs)
                {
                    FinalLongNoteJudgement(note, currentTimeMs);
                    note.isInputed = true;
                }
            }
        }
    }

    private void ShowLongNoteTickJudgement(NoteClass note)
    {
        float totalTime = 60000f / noteGenerator.BPM * note.length;
        float ratio = note.pressedTime / totalTime;

        string currentJudgement = GetLongNoteJudgement(ratio, note.startJudgement);

        // 점수 반영 없이 텍스트만 표시
        StartCoroutine(UIManager.JudgementTextShower(currentJudgement, 0, note.position));
    }

    private void FinalLongNoteJudgement(NoteClass note, double currentTimeMs)
    {
        float totalTime = 60000f / noteGenerator.BPM * note.length;
        float ratio = note.pressedTime / totalTime;

        string finalJudgement = GetLongNoteJudgement(ratio, note.startJudgement);

        // longNote 제거 (기다란 노트)
        if (note.longObject != null)
        {
            Destroy(note.longObject);
        }

        // 최종 판정: 점수와 카운트 반영
        PerformAction(note, finalJudgement, currentTimeMs);

        // 콤보 처리
        if (finalJudgement == "PerfectP" || finalJudgement == "Perfect" || finalJudgement == "Great")
        {
            // 콤보는 시작 판정에서 이미 추가했으므로 유지
        }
        else
        {
            ClearCombo();
        }
    }

    private string GetLongNoteJudgement(float ratio, string startJudgement)
    {
        // 비율에 따른 판정
        string judgementByRatio;

        Debug.Log($"Long Note Ratio: {ratio}");
        if (ratio >= 1f)
        {
            judgementByRatio = "PerfectP";
        }
        else if (ratio >= 0.8f)
        {
            judgementByRatio = "Perfect";
        }
        else if (ratio >= 0.5f)
        {
            judgementByRatio = "Great";
        }
        else
        {
            judgementByRatio = "Good";
        }

        // 시작 판정을 넘을 수 없음
        return GetWorseJudgement(startJudgement, judgementByRatio);
    }

    private string GetWorseJudgement(string judgement1, string judgement2)
    {
        // 판정 우선순위: PerfectP > Perfect > Great > Good > Bad
        Dictionary<string, int> priority = new Dictionary<string, int>
        {
            { "PerfectP", 5 },
            { "Perfect", 4 },
            { "Great", 3 },
            { "Good", 2 },
            { "Bad", 1 }
        };

        int p1 = priority.ContainsKey(judgement1) ? priority[judgement1] : 0;
        int p2 = priority.ContainsKey(judgement2) ? priority[judgement2] : 0;

        return p1 < p2 ? judgement1 : judgement2; // 더 낮은(나쁜) 판정 반환
    }

}
