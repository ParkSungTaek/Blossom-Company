using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUI : UI_Scene
{

    public static GameUI Instance { get; private set; }
    GameUI() { }



    ClearRwrdData clearRwrdData = new ClearRwrdData();
    bool _clear = false;
    public bool Clear { get { return _clear; } }
    
    GameObject UnbeatBlock;
    TimeSlider _timeSlider;
    public TimeSlider timeSlider { get { return _timeSlider; } }

    int jump { get { return clearRwrdData.Jump; } set { clearRwrdData.Jump = value > 0 ? value : 0; ClearCheck(); } }
    int skip { get { return clearRwrdData.Skip; } set { clearRwrdData.Skip = value > 0 ? value : 0; ClearCheck(); } }
    int bloom { get { return clearRwrdData.Bloom; } set { clearRwrdData.Bloom = value > 0 ? value : 0; ClearCheck(); } }
    public int ClearReward_GoldBranch { get { return clearRwrdData.ClearReward_GoldBranch; } }
    void JsonDeepCopy()
    {
        clearRwrdData.Quest = GameManager.InGameDataManager.ClearRwrdHandler[GameManager.InGameDataManager.QuestIDX].Quest;
        clearRwrdData.Jump = GameManager.InGameDataManager.ClearRwrdHandler[GameManager.InGameDataManager.QuestIDX].Jump;
        clearRwrdData.Skip = GameManager.InGameDataManager.ClearRwrdHandler[GameManager.InGameDataManager.QuestIDX].Skip;
        clearRwrdData.Bloom = GameManager.InGameDataManager.ClearRwrdHandler[GameManager.InGameDataManager.QuestIDX].Bloom;
        clearRwrdData.ClearReward_GoldBranch = GameManager.InGameDataManager.ClearRwrdHandler[GameManager.InGameDataManager.QuestIDX].ClearReward_GoldBranch;

    }



    void ClearCheck()
    {
        if (!_clear)
        {
            if (jump == 0 && skip == 0 && bloom == 0)
            {
                _clear = true;
                GameManager.InGameDataManager.QuestIDX++;
            }
        }
    }

    enum Buttons
    {
        JumpBtn,
        SkipBtn,
    }

    enum Texts
    {
        ScoreTxt,
        JumpCnt,
        SkipCnt,
        BloomCnt,
    }

    void Awake()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();

        if (Instance == null)
        {
            Instance = this;
        }
        //Object 바인드
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));

        //Btn에 이벤트 연결
        BindEvent(GetButton((int)Buttons.JumpBtn).gameObject, Btn_Jump);
        BindEvent(GetButton((int)Buttons.SkipBtn).gameObject, Btn_Skip);

        JsonDeepCopy();

        GetText((int)Texts.ScoreTxt).text = $"QUEST: {clearRwrdData.Quest}";
        GetText((int)Texts.JumpCnt).text = $"Jump: {jump}";
        GetText((int)Texts.SkipCnt).text = $"Skip: {skip}";
        GetText((int)Texts.BloomCnt).text = $"Bloom: {bloom}";
        UnbeatBlock = GameObject.Find("UnbeatBlock");
        UnbeatBlock.SetActive(false);
        _timeSlider = Util.FindChild(gameObject, "Timeslider", true).GetComponent<TimeSlider>();




    }

    public bool isJumpActive { get; set; } = true;
    public bool isSkipActive { get; set; } = true;


    GameObject Lifeicon;
    public GameObject LifeIcon { get { return Lifeicon; } set { Lifeicon = value; } }



    void Btn_Jump(PointerEventData evt)
    {

        // 실제로 점프가 되는 상황 에서만! 점프 사운드가 나는게 맞냐?
        // 버튼을 누르면 무조껀 사운드는 나고 점프가 되냐 마냐는 사운드 알바는 아니다. 

        GameManager.SoundManager.Play(Define.SFX.Jump_01);


        if (isJumpActive)
        {
            Debug.Log("JumpBtn눌림");
            GameManager.InGameDataManager.NowState.JumpCnt++;
            jump--;
            GetText((int)Texts.JumpCnt).text = $"Jump: {jump}";
            GameManager.InGameDataManager.Player.GetComponent<PlayerController>().Jump();
        }
        else
        {
            if (!TileController.IsMoving)
            {
                //Background move라는 Action(Delegate 즉 대행자의 일종)에 값이 있으면 실행 BackGround에서 대행자가 처리할 일을 더 해 준다.
                TileController.Instance.BackGroundMove?.Invoke();
                GameManager.InGameDataManager.Player.GetComponent<PlayerController>().Skip();
                GameManager.InGameDataManager.NowState.SkipCnt++;
                skip--;
                GetText((int)Texts.SkipCnt).text = $"Skip: {skip}";

                TileController.Instance.MoveTiles();

            }
        }


    }
    void Btn_Skip(PointerEventData evt)
    {

        GameManager.SoundManager.Play(Define.SFX.Skip_01);

        if (isSkipActive)
        {
            if (!TileController.IsMoving)
            {
                //Background move라는 Action(Delegate 즉 대행자의 일종)에 값이 있으면 실행 BackGround에서 대행자가 처리할 일을 더 해 준다.
                TileController.Instance.BackGroundMove?.Invoke();
                GameManager.InGameDataManager.Player.GetComponent<PlayerController>().Skip();
                GameManager.InGameDataManager.NowState.SkipCnt++;
                skip--;
                GetText((int)Texts.SkipCnt).text = $"Skip: {skip}";

                TileController.Instance.MoveTiles();

            }
        }
        else
        {
            Debug.Log("JumpBtn눌림");

            GameManager.InGameDataManager.NowState.JumpCnt++;
            jump--;
            GetText((int)Texts.JumpCnt).text = $"Jump: {jump}";
            GameManager.InGameDataManager.Player.GetComponent<PlayerController>().Jump();
        }

    }

    public void BloomCnt()
    {
        GameManager.InGameDataManager.NowState.BloomCnt++;
        bloom--;
        GetText((int)Texts.BloomCnt).text = $"BloomCnt: {bloom}";

    }

    #region ItemEffect Area


    #region TimeFreezeItem

    public float freezeDuration = 5f;
    private bool isTimeFrozen = false;
    Coroutine TimeFreezeCoroutine;

    public void TimeFreeze()
    {
        Debug.Log("TimeFreezeSTart");
        if (isTimeFrozen == false) //만약 istumefrozen이 작동할 수 없다면
        {
            _timeSlider.StopTimer();
            isTimeFrozen = true;// 시간을 얼리는 조건 true로 변경
            TimeFreezeCoroutine = StartCoroutine(ResumeTimeAfterDelay(freezeDuration));//코루틴 실행

        }
        else //갱신 지금부터 4초
        {
            StopCoroutine(TimeFreezeCoroutine);
            TimeFreezeCoroutine = StartCoroutine(ResumeTimeAfterDelay(freezeDuration));//코루틴 실행
        }

    }

    private IEnumerator ResumeTimeAfterDelay(float delay)//5초 뒤에 작동
    {
        yield return new WaitForSeconds(delay);  //  변경된 부분>> WaitForSeconds를 사용하도록 변경/
                                                 //  WaitForSeconds(5.0f); >5초 동안 멈춘 후 메세지 출력(timescale의 영향 받지X)

        //originalValue = slider.value;//s.v를 o.v값에 대입>>그러므로 ov는 슬라이더가 멈춰도 이전의 값을 기억하게 됨.
        //
        //slider.interactable = false; //아이템 작동 기간 동안 Slider의 상호작용 중지(슬라이더 멈추는 것)
        isTimeFrozen = false; // 변경된 부분>> 시간멈춤을 해제할 때 isTimeFrozen을 false로 변경
        //slider.interactable = true;//아이템 작동 후 다시 Slider 상호작용0

        _timeSlider.ResetSpeed();

        Debug.Log("END TimeFreezeItem ");

    }

    #endregion TimeFreezeItem


    #region SkipJumpSwapItem

    private bool isEffectActive = false;
    private Coroutine SkipJumpSwapItemCoroutine;
    GameObject SkipJumpSwapItemIcon;

    float _swapTime = 10.0f;


    public void SkipJumpSwapItem()
    {
        if (isEffectActive == false)    //효과 적용중아님  
        {
            isEffectActive = true;
            isJumpActive = false;
            isSkipActive = false;

            SkipJumpSwapItemCoroutine = StartCoroutine(isCounting());
        }
        else
        {

            StopCoroutine(SkipJumpSwapItemCoroutine);

            Debug.Log("재시작");

            isEffectActive = true;
            isJumpActive = false;
            isSkipActive = false;

            SkipJumpSwapItemCoroutine = StartCoroutine(isCounting());

        }
    }


    /// <summary>
    /// 버튼 기능 10초? 간 바뀜
    /// </summary>
    /// <returns></returns>
    private IEnumerator isCounting()

    {
        Debug.Log("시작");

        if (SkipJumpSwapItemIcon == null)   //중복 생성 방지
        {
            SkipJumpSwapItemIcon = GameManager.ResourceManager.Instantiate("ItemTypes/icon_SkipJumpSwap");
        }

        SkipJumpSwapItemIcon.SetActive(true);

        yield return new WaitForSecondsRealtime(_swapTime);

        Debug.Log("끝");
        isEffectActive = false;


        isJumpActive = true;
        isSkipActive = true;

        SkipJumpSwapItemIcon.SetActive(false);

    }

    #endregion SkipJumpSwapItem


    ///
    #region HideRemainJumpItem

    private bool isHideActive = false;
    Coroutine HideRemainJumpItemCoroutine;
    GameObject HideRemainJumpItemIcon;
    GameObject Obstacle;

    float _hideTime = 5.0f;


    public void HideRemainJumpItem()
    {
        if (isHideActive == false)    //효과 적용중아님  
        {
            isHideActive = true;

            HideRemainJumpItemCoroutine = StartCoroutine(HideTile());
        }
        else    //효과 적용 중
        {

            StopCoroutine(HideRemainJumpItemCoroutine);
            HideRemainJumpItemCoroutine = StartCoroutine(HideTile());

        }
    }

    /// <summary>
    /// 시야 방해물이 5초간 나타남
    /// </summary>
    private IEnumerator HideTile()

    {

        if (Obstacle == null)   //중복 생성 방지
        {
            Obstacle = GameManager.ResourceManager.Instantiate("ItemTypes/obstacle");
        }


        yield return new WaitForSecondsRealtime(_hideTime);

        isHideActive = false;

        if (Obstacle != null)
        {
            Destroy(Obstacle);
        }
    }


    #endregion HideRemainJumpItem

    #region UnbeatableItem

    public float unbeatableDuration = 10f; // 무적 지속 시간
    private bool isUnbeatable;//bool 설정
    private float originalGravityScale;
    private Rigidbody2D WingWingRigidbody;
    GameObject UnbeatableItemIcon;
    Coroutine UnbeatCoroutine;

    public void Unbeatable()
    {
        Debug.Log("Unbeatable() 시작");
        if (isUnbeatable == false)
        {
            isUnbeatable = true;
            UnbeatBlock.SetActive(true);
            UnbeatCoroutine = StartCoroutine(ResumeUnbeatableAfterDelay());
            //10초 뒤에 코루틴 실행
        }
        else
        {
            StopCoroutine(UnbeatCoroutine);
            isUnbeatable = true;
            UnbeatBlock.SetActive(true);
            UnbeatCoroutine = StartCoroutine(ResumeUnbeatableAfterDelay());

        }

    }


    private IEnumerator ResumeUnbeatableAfterDelay(float delay = 10f)
    {
        UnbeatableItemIcon = GameManager.ResourceManager.Instantiate("ItemTypes/icon_Unbeatable");//아이콘 띄우기
        UnbeatableItemIcon.SetActive(true);

        yield return new WaitForSeconds(delay);

        isUnbeatable = false;
        UnbeatBlock.SetActive(false);
        UnbeatableItemIcon.SetActive(false);//아이콘 제거
    }


    public void PlusTime()
    { 


    }

}


#endregion  UnbeatableItem



#endregion ItemEffect Area
