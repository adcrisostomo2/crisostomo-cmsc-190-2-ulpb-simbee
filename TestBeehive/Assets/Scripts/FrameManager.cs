using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;
using System.IO;

public class FrameManager : MonoBehaviour
{
    public Transform[] frame_slots;
    public GameObject[] frame_models;
    public HiveFrame[] langstroth_frames = new HiveFrame[10];
    public GameObject main_cam_ref_point;
    public CanvasGroup main_canvas;
    public Text[] data_monitors;

    public static bool automatic = true;
    public static bool manual = false;

    private Transform parent;
    private Coroutine simulation = null;
    private bool isSimulating = false;
    private IEnumerator sim = null;
    private int simIndex = -1;

    // frame "attributes"
    private GameObject[] frames = new GameObject[10];
    private bool[] has_frames = new bool[10];
    private bool[] is_raised = new bool[10];
    private bool[] is_highlighted = new bool[10];

    // placeholder frame instance
    private GameObject placeholder_frame;

    private const int FRAME_LIMIT = 10;
    private const float GO_UP = 18f;
    private const float GO_DOWN = -18f;
    private string filepath;
    private List<List<string>> commands;
    private bool clicked = false;
    private bool gameIsPaused = false;
    private int frame_count;
    private int langframe_count;
    private int prev_selected_frame;
    private int selected_frame;
    private int current_frame_type;
    private int current_back_most_frame_slot;

    //For identifying if hive is transforming F to S Frame
    private bool isMaking = false;

    private bool isLaying = false;
    private bool isBeekeeping = false;
    private int qLoc = 0;
    private int eggTotal = 0;
    private int larvaeTotal = 0;
    private int pupaTotal = 0;
    private int workersTotal = 0;
    private int[] workers = new int[40000];
    private float foodTotal = 0f;
    private float broodTotal = 0f;
    private List<float> foodList = new List<float>();
    
    //For hive clock
    private int day = 0;
    private bool isInitialSetup = false;
    public static float sleepInterval = 1f;

    private StreamWriter sw;

    public static bool isReset = true;
    public static bool hasRead = false;
    
    // Start is called before the first frame update
    void Start()
    {
        if (isReset)
        {
            Reset();
        }
        isReset = false;

        placeholder_frame = new GameObject();

        initializeHive();
    }

    void initializeHive()
    {
        for (int i = 0; i < 10; i++)
        {
            has_frames[i] = false;
            is_raised[i] = false;
            is_highlighted[i] = false;
            Destroy(frames[i]);
            langstroth_frames[i] = new HiveFrame();
            frames[i] = placeholder_frame;
        }

        selected_frame = -1;
        prev_selected_frame = selected_frame;
        frame_count = 0;
        langframe_count = 0;
        current_frame_type = 0;
        current_back_most_frame_slot = 0;
    }

    public void LoadFrames()
    {
        initializeHive();

        for (int i = 0; i < 10; i++)
        {
            int frame_type = Random.Range(0, 4);
            AddFrame(i, frame_type);

            Debug.Log("FRAME_COUNT: " + (i+1) + "Added Frame: " + i + " " + frame_type);
        }
    }

    private void Reset()
    {
        isReset = false;
        Start();
    }

    void Update()
    { 
        if (!clicked && Input.GetMouseButtonDown(0))
        {
            clicked = true;
            Debug.Log("Tap/Click Detected!");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Hit Something!");
                //  || (hit.transform.name == "ButtonsPanel" || hit.transform.name == "SpecificFrameButtonsPanel")
                if (IsPointerOverUIObject())
                {
                    Debug.Log("UIObject: " + hit.transform.name);
                    if (selected_frame > -1 && is_highlighted[selected_frame])
                    {
                        // unhighlight selected frame
                        Renderer frame_renderer = frames[selected_frame].GetComponent<Renderer>();
                        Color col = frame_renderer.material.GetColor("_Color");
                        frame_renderer.material.SetColor("_Color", new Color(col.r - 0.35f, col.g - 0.35f, col.b - 0.35f, 1f));
                        is_highlighted[selected_frame] = false;

                        prev_selected_frame = selected_frame;
                        selected_frame = -1;
                    }
                    // don't interact with specific frame
                    return;
                }
                else
                {
                    Debug.Log("Check if it hit Frame: " + hit.transform.name);
                    for (int i = 0; i < FRAME_LIMIT; i++)
                    {
                        if (has_frames[i] && frames[i].transform.name == hit.transform.name)
                        {
                            Debug.Log(i + " : " + frames[i].transform.name);

                            prev_selected_frame = selected_frame;
                            selected_frame = i;

                            if (prev_selected_frame > -1 && is_highlighted[prev_selected_frame])
                            {
                                // unhighlight previously selected frame
                                Renderer frame_renderer = frames[prev_selected_frame].GetComponent<Renderer>();
                                Color col = frame_renderer.material.GetColor("_Color");
                                frame_renderer.material.SetColor("_Color", new Color(col.r - 0.35f, col.g - 0.35f, col.b - 0.35f, 1f));
                                is_highlighted[prev_selected_frame] = false;
                            }

                            if (!is_highlighted[selected_frame])
                            {
                                // highlight selected frame
                                Renderer frame_renderer = frames[selected_frame].GetComponent<Renderer>();
                                Color col = frame_renderer.material.GetColor("_Color");
                                frame_renderer.material.SetColor("_Color", new Color(col.r + 0.35f, col.g + 0.35f, col.b + 0.35f, 1f));
                                is_highlighted[selected_frame] = true;
                            }

                            // stop here so if there are objects behind the selected object and are hit by the Raycast, they won't be processed
                            break;
                        }
                        else if (i + 1 == FRAME_LIMIT && has_frames[i] && frames[i].transform.name != hit.transform.name)
                        {
                            if (selected_frame > -1 && is_highlighted[selected_frame])
                            {
                                // unhighlight selected frame
                                Renderer frame_renderer = frames[selected_frame].GetComponent<Renderer>();
                                Color col = frame_renderer.material.GetColor("_Color");
                                frame_renderer.material.SetColor("_Color", new Color(col.r - 0.35f, col.g - 0.35f, col.b - 0.35f, 1f));
                                is_highlighted[selected_frame] = false;

                                prev_selected_frame = selected_frame;
                                selected_frame = -1;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            clicked = false;
        }
    }
    
    public void NextDay()
    {
        isBeekeeping = false;
    }

    private IEnumerator Simulate()
    {
        //// Initial hive setup here
        //if (isInitialSetup == false)
        //{
        //    InitializeWorkers();

        //    //langstroth_frames[0].type = 1;
        //    //langstroth_frames[1].type = 2;
        //    //langstroth_frames[2].type = 3;
        //    //langstroth_frames[3].type = 2;

        //    CreateInitialFrameValues(0, 1);
        //    CreateInitialFrameValues(1, 2);
        //    CreateInitialFrameValues(2, 3);
        //    CreateInitialFrameValues(3, 2);

        //    qLoc = 33;

        //    NewWorkers(3200);
        //    //automatic = true;
        //}
        //isInitialSetup = true;
        
        while (day < 180)
        {
            yield return new WaitForSeconds(sleepInterval);

            day += 1;

            //Laying of eggs
            Queen();

            //Incrementation of cell values
            UpdateFrameQuantities();

            if (manual)
            {
                isBeekeeping = true;
                while (isBeekeeping)
                { 
                    yield return new WaitForSeconds(0.1f);
                }
            }
            //else if (automatic)
            //{
                /*
                * Checks cell values then
                * changes the frame type depending on necessity
                */
            UpdateFrameCellOrType();
            //}

            //Updates Workers' lifespan
            UpdateWorkers();

            //For getting total Honey and brood(To be sure values are correct)
            UpdateTotalFrameQuantities();

            /*// Update Table & Graph
            updateTable();
            updateGraph();*/

            Debug.Log("Day:" + day + " Food:" + foodTotal + " Egg:" + eggTotal + " Pupa:" + pupaTotal + " Workers:" + workersTotal + " Queen:" + qLoc + " Frame Total: " + langframe_count + " Rendered Frame Total: " + frame_count);

            data_monitors[0].text = "DAY " + day;
            data_monitors[1].text = "Food: " + foodTotal;
            data_monitors[2].text = "Egg: " + eggTotal;
            data_monitors[3].text = "Pupa: " + pupaTotal;
            data_monitors[4].text = "Workers: " + workersTotal;
            data_monitors[5].text = "Total Population: " + (workersTotal + eggTotal + pupaTotal + larvaeTotal);

            HiveGraph.instance.AppendToValueList((int) (workersTotal+eggTotal+pupaTotal+larvaeTotal), simIndex);
            //try
            //{
            //    sw.WriteLine("Day:" + day + " Food:" + foodTotal + " Egg:" + eggTotal + " Pupa:" + pupaTotal + " Workers:" + workersTotal + " Queen:" + qLoc + " Frame Total: " + langframe_count);
            //}
            //catch (System.Exception e)
            //{
            //    Debug.Log("Exception: " + e.Message);
            //}
        }
    }

    public void StartSimulation()
    {
        simIndex += 1;
        isMaking = false;

        isLaying = false;
        isBeekeeping = false;
        qLoc = 0;
        eggTotal = 0;
        larvaeTotal = 0;
        pupaTotal = 0;
        workersTotal = 0;
        workers = new int[40000];
        foodTotal = 0f;
        broodTotal = 0f;
        foodList = new List<float>();

        day = 0;
        isInitialSetup = false;
        sleepInterval = 1f;

        initializeHive();

        //for (int i = 0; i < 10; i++)
        //{
        //    DeleteFrame(i);
        //}

        //langstroth_frames = new HiveFrame[10];

        // Initial hive setup here
        if (isInitialSetup == false)
        {
            InitializeWorkers();
            
            CreateInitialFrameValues(0, 1);
            CreateInitialFrameValues(1, 2);
            CreateInitialFrameValues(2, 3);
            CreateInitialFrameValues(3, 2);

            qLoc = 33;

            NewWorkers(3200);
        }
        isInitialSetup = true;

        if (isSimulating)
        {
            StopCoroutine(sim);
            isSimulating = false;
        }

        sim = Simulate();
        StartCoroutine(sim);
        isSimulating = true;
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return (results.Count > 0) ? true : false;
    }

    public void SetCurrentFrameTypeToFoodAndAdd()
    {
        this.current_frame_type = 1;
        this.CreateFrame();
    }

    public void SetCurrentFrameTypeToOpenBroodAndAdd()
    {
        this.current_frame_type = 2;
        this.CreateFrame();
    }

    public void SetCurrentFrameTypeToSealedBroodAndAdd()
    {
        this.current_frame_type = 3;
        this.CreateFrame();
    }

    public void SetCurrentFrameTypeToStickyAndAdd()
    {
        this.current_frame_type = 4;
        this.CreateFrame();
    }

    public void SetCurrentFrameTypeToFoundationAndAdd()
    {
        this.current_frame_type = 5;
        this.CreateFrame();
    }

    public void SetCurrentBackmostFrameSlot()
    {
        int i = 0;
        int escaper = 0;
        bool found = false;
        while (!found && escaper < 100)
        {
            if (i > 9)
            {
                i = 0;
            }
            else if (i < 0)
            {
                i = 9;
            }
            if (!has_frames[i])
            {
                Debug.Log("New backward most frame slot at index " + i);
                this.current_back_most_frame_slot = i;

                found = true;
            }
            else
            {
                i += 1;
            }
            escaper += 1;
        }
        //for (int i = current_back_most_frame_slot; i < FRAME_LIMIT; i++)
        //{
        //    if (!has_frames[i])
        //    {
        //        this.current_back_most_frame_slot = i;
        //        break;
        //    }
        //}
    }

    public string GetFrameTypeName(int index)
    {
        string name = frames[index].transform.name;
        string substring = name.Substring(0, name.Length - 8);

        Debug.Log(substring);
        return substring;
    }

    public void ToggleAllFrameLabels()
    {
        for (int i = 0; i < frame_count; i++)
        {
            Debug.Log(GetFrameTypeName(i));
            GameObject frameLabel = frames[i].gameObject.transform.Find(GetFrameTypeName(i)).gameObject;
            frameLabel.SetActive(!frameLabel.activeSelf);
        }
    }
    
    public void AddFrame()
    {
        //Debug.Log(current_back_most_frame_slot);
        //Debug.Log(has_frames.Length);
        if (frame_count < 10 && !has_frames[current_back_most_frame_slot])
        {
            //Debug.Log("Current Left Most Frame Slot: " + current_back_most_frame_slot);
            frames[current_back_most_frame_slot] = Instantiate(frame_models[current_frame_type], parent);
            frames[current_back_most_frame_slot].GetComponent<Transform>().SetPositionAndRotation(frame_slots[current_back_most_frame_slot].position, frame_slots[current_back_most_frame_slot].rotation);
            //frames[current_back_most_frame_slot] = Instantiate(frame_models[current_frame_type], frame_slots[current_back_most_frame_slot].position, frame_slots[current_back_most_frame_slot].rotation) as GameObject;

            // rename frame; make new name unique
            frames[current_back_most_frame_slot].transform.name = string.Concat(frames[current_back_most_frame_slot].transform.name, current_back_most_frame_slot);
            
            has_frames[current_back_most_frame_slot] = true;
            SetCurrentBackmostFrameSlot();
            frame_count += 1;
            Debug.Log("ADD BUTTON CLICKED AND FRAME ADDED AT " + current_back_most_frame_slot + "!");
        }
        else
        {
            return;
        }
    }

    public void AddFrame(int index, int frame_type)
    {
        //Debug.Log(current_back_most_frame_slot);
        //Debug.Log(has_frames.Length);
        if (frame_count < 10 && !has_frames[index])
        {
            //Debug.Log("Current Left Most Frame Slot: " + current_back_most_frame_slot);
            //frames[index] = Instantiate(frame_models[frame_type], frame_slots[index].position, frame_slots[index].rotation);
            frames[index] = Instantiate(frame_models[frame_type], parent);
            frames[index].GetComponent<Transform>().SetPositionAndRotation(frame_slots[index].position, frame_slots[index].rotation);

            // rename frame; make new name unique
            frames[index].transform.name = string.Concat(frames[index].transform.name, index);

            has_frames[index] = true;
            SetCurrentBackmostFrameSlot();
            frame_count += 1;
            //Debug.Log("(index) FRAME ADDED AT " + index + "!");
        }
        else
        {
            return;
        }
    }

    public void RemoveFrame(int index)
    {
        if (has_frames[index])
        {
            Debug.Log("////////////////////////////////////////////");
            Destroy(frames[index]);
            Debug.Log("Deleted frames["+index+"]: " + frames[index]);
            Debug.Log("////////////////////////////////////////////");
            has_frames[index] = false;
            is_raised[index] = false;
            is_highlighted[index] = false;
            frame_count -= 1;
            SetCurrentBackmostFrameSlot();
            Debug.Log("("+index+") FRAME REMOVED!");
        }
    }

    public void RemoveSelectedFrame()
    {
        if (selected_frame > -1 && has_frames[selected_frame])
        {
            Debug.Log("frame_count: " + frame_count);
            DeleteFrame(selected_frame);

            Destroy(frames[selected_frame]);
            has_frames[selected_frame] = false;
            is_raised[selected_frame] = false;
            is_highlighted[selected_frame] = false;
            frame_count -= 1;
            SetCurrentBackmostFrameSlot();
            Debug.Log("REMOVE BUTTON CLICKED AND FRAME REMOVED!");
        }
    }

    public void RaiseFrame(int index)
    {
        if (!is_raised[index])
        {
            frames[index].transform.Translate(0f, 0f, GO_UP);
            is_raised[index] = true;
        }
    }

    public void RaiseSelectedFrame()
    {
        if (selected_frame > -1 && !is_raised[selected_frame])
        {
            frames[selected_frame].transform.Translate(0f, 0f, GO_UP);
            is_raised[selected_frame] = true;
        }
    }

    public void LowerSelectedFrame()
    {
        if (selected_frame > -1 && is_raised[selected_frame])
        {
            frames[selected_frame].transform.Translate(0f, 0f, GO_DOWN);
            is_raised[selected_frame] = false;
        }
    }

    public void LowerFrame(int index)
    {
        if (is_raised[index])
        {
            frames[index].transform.Translate(0f, 0f, GO_DOWN);
            is_raised[index] = false;
        }
    }

    public void RaiseAllFrames()
    {
        for (int i = 0; i < FRAME_LIMIT; i++)
        {
            if (has_frames[i])
            {
                RaiseFrame(i);
            }
        }
    }

    public void LowerAllFrames()
    {
        for (int i = 0; i < FRAME_LIMIT; i++)
        {
            if (has_frames[i])
            {
                LowerFrame(i);
            }
        }
    }

    public void MoveSelectedFrameBack()
    {
        if (selected_frame > -1)
        {
            int i = selected_frame - 1;
            bool found = false;
            int escaper = 0;

            while (!found && escaper < 100)
            {
                if (i > 9)
                {
                    i = 0;
                }
                else if (i < 0)
                {
                    i = 9;
                }
                if (!has_frames[i])
                {
                    Debug.Log("Found! at index " + i);

                    // swap frame data
                    HiveFrameWrapper hive_frame_wrapper_a = new HiveFrameWrapper(langstroth_frames[selected_frame]);
                    HiveFrameWrapper hive_frame_wrapper_b = new HiveFrameWrapper(langstroth_frames[i]);

                    //SwapFrameFunc(i, i + 1);
                    SwapFrameFunc(hive_frame_wrapper_a, hive_frame_wrapper_b);

                    langstroth_frames[selected_frame] = hive_frame_wrapper_a.hive_frame;
                    langstroth_frames[i] = hive_frame_wrapper_b.hive_frame;

                    has_frames[i] = has_frames[selected_frame];
                    is_highlighted[i] = is_highlighted[selected_frame];
                    is_raised[i] = is_raised[selected_frame];
                    has_frames[selected_frame] = false;
                    is_highlighted[selected_frame] = false;
                    is_raised[selected_frame] = false;

                    frames[i] = frames[selected_frame];
                    Vector3 position = frame_slots[i].position;
                    position.y = frames[selected_frame].transform.position.y;
                    frames[i].transform.SetPositionAndRotation(position, frame_slots[i].rotation);
                    selected_frame = i;

                    SetCurrentBackmostFrameSlot();
                    found = true;
                }
                else
                {
                    i -= 1;
                }
                escaper += 1;
            }
        }
    }

    public void MoveSelectedFrameFront()
    {
        if (selected_frame > -1)
        {
            int i = selected_frame + 1;
            bool found = false;
            int escaper = 0;

            while (!found && escaper < 100)
            {
                if (i > 9)
                {
                    i = 0;
                }
                else if (i < 0)
                {
                    i = 9;
                }
                if (!has_frames[i])
                {
                    Debug.Log("Found! at index " + i);

                    // swap frame data
                    HiveFrameWrapper hive_frame_wrapper_a = new HiveFrameWrapper(langstroth_frames[selected_frame]);
                    HiveFrameWrapper hive_frame_wrapper_b = new HiveFrameWrapper(langstroth_frames[i]);

                    //SwapFrameFunc(i, i + 1);
                    SwapFrameFunc(hive_frame_wrapper_a, hive_frame_wrapper_b);

                    langstroth_frames[selected_frame] = hive_frame_wrapper_a.hive_frame;
                    langstroth_frames[i] = hive_frame_wrapper_b.hive_frame;

                    has_frames[i] = has_frames[selected_frame];
                    is_highlighted[i] = is_highlighted[selected_frame];
                    is_raised[i] = is_raised[selected_frame];
                    has_frames[selected_frame] = false;
                    is_highlighted[selected_frame] = false;
                    is_raised[selected_frame] = false;

                    frames[i] = frames[selected_frame];
                    Vector3 position = frame_slots[i].position;
                    position.y = frames[selected_frame].transform.position.y;
                    frames[i].transform.SetPositionAndRotation(position, frame_slots[i].rotation);
                    selected_frame = i;

                    Debug.Log("selected_frame: " + selected_frame);

                    SetCurrentBackmostFrameSlot();
                    found = true;
                }
                else
                {
                    i += 1;
                }
                escaper += 1;
            }
        }
    }

    /* SIMULATION FUNCTIONS */
    void Queen()
    {
        if (qLoc % 10 < 3)
        {
            qLoc += 1;
        }
    }

    void QueenLayEggs()
    {
        if (isLaying == false)
        {
            if (langstroth_frames[1].type == 4 && (langframe_count > 3))
            {
                Debug.Log("Transform S to O Frame");
                Debug.Log("go to [1]");
                qLoc = 20;
                isLaying = true;
                //RemoveFrame(1);
                //createInitialFrameValues(1,2);
            }
            else if (langstroth_frames[2].type == 4 && (langframe_count > 3))
            {
                Debug.Log("Transform S to O Frame");
                Debug.Log("go to [2]");
                qLoc = 30;
                isLaying = true;
                //RemoveFrame(2);
                //createInitialFrameValues(2,2);
            }
            else if (langstroth_frames[3].type == 4 && langframe_count > 3)
            {
                Debug.Log("Transform S to O Frame");
                Debug.Log("go to [3]");
                qLoc = 40;
                isLaying = true;
                //RemoveFrame(3);
                //createInitialFrameValues(3,2);
            }
            else if (langstroth_frames[4].type == 4 && langframe_count > 5)
            {
                //RemoveFrame(4);
                //createInitialFrameValues(4,2);
                qLoc = 50;
                isLaying = true;
            }
            else if ((langstroth_frames[5].type == 4 && langframe_count > 7) && CheckFrameContinuity(9) == true)
            {
                //RemoveFrame(5);
                //createInitialFrameValues(5,2);
                qLoc = 60;
                isLaying = true;
            }
            else if ((langstroth_frames[6].type == 4 && langframe_count > 8) && CheckFrameContinuity(9) == true)
            {
                //RemoveFrame(6);
                //createInitialFrameValues(6,2);
                qLoc = 70;
                isLaying = true;
            }
        }
    }

    void UpdateFrameQuantities()
    {
        for (int i = 0; i < 40; i++)
        {
            for (int j = 0; j < 40; j++)
            {
                langstroth_frames[0].cellA[i, j] += 1;
                langstroth_frames[0].cellB[i, j] += 1;
                langstroth_frames[1].cellA[i, j] += 1;
                langstroth_frames[1].cellB[i, j] += 1;
                langstroth_frames[2].cellA[i, j] += 1;
                langstroth_frames[2].cellB[i, j] += 1;
                langstroth_frames[3].cellA[i, j] += 1;
                langstroth_frames[3].cellB[i, j] += 1;
                langstroth_frames[4].cellA[i, j] += 1;
                langstroth_frames[4].cellB[i, j] += 1;
                langstroth_frames[5].cellA[i, j] += 1;
                langstroth_frames[5].cellB[i, j] += 1;
                langstroth_frames[6].cellA[i, j] += 1;
                langstroth_frames[6].cellB[i, j] += 1;
                langstroth_frames[7].cellA[i, j] += 1;
                langstroth_frames[7].cellB[i, j] += 1;
                langstroth_frames[8].cellA[i, j] += 1;
                langstroth_frames[8].cellB[i, j] += 1;
                langstroth_frames[9].cellA[i, j] += 1;
                langstroth_frames[9].cellB[i, j] += 1;
            }
        }
    }

    public void CreateFrame()
    {
        CreateInitialFrameValues(current_back_most_frame_slot, current_frame_type);
    }

    void CreateInitialFrameValues(int frame_index, int frame_type)
    {
        if (langstroth_frames[frame_index].type == 0)
        { // slot is empty
            langframe_count += 1;
        }

        //Food
        if (frame_type == 1)
        {
            langstroth_frames[frame_index].type = 1;
            langstroth_frames[frame_index].foodA = 6400;
            langstroth_frames[frame_index].foodB = 6400;
            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 40; j++)
                {
                    langstroth_frames[frame_index].cellA[i, j] = 1000;
                    langstroth_frames[frame_index].cellB[i, j] = 1000;
                }
            }
            // render Food frame
            AddFrame(frame_index, frame_type);
        }
        //Egg/Larvae/Open
        else if (frame_type == 2)
        {
            langstroth_frames[frame_index].type = 2;
            langstroth_frames[frame_index].eggA = 1600;
            langstroth_frames[frame_index].eggB = 1600;
            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 40; j++)
                {
                    langstroth_frames[frame_index].cellA[i, j] = 1;
                    langstroth_frames[frame_index].cellB[i, j] = 1;
                }
            }
            // render Open frame
            AddFrame(frame_index, frame_type);
        }
        //Pupa/Larvae/Sealed
        else if (frame_type == 3)
        {
            langstroth_frames[frame_index].type = 3;
            langstroth_frames[frame_index].pupaA = 1600;
            langstroth_frames[frame_index].pupaB = 1600;
            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 40; j++)
                {
                    langstroth_frames[frame_index].cellA[i, j] = 10;
                    langstroth_frames[frame_index].cellB[i, j] = 10;
                }
            }
            // render Sealed frame
            AddFrame(frame_index, frame_type);
        }
        //Sticky
        else if (frame_type == 4)
        {
            langstroth_frames[frame_index].type = 4;
            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 40; j++)
                {
                    langstroth_frames[frame_index].cellA[i, j] = 115;
                    langstroth_frames[frame_index].cellB[i, j] = 115;
                }
            }
            // render Sticky frame
            AddFrame(frame_index, frame_type);
        }
        //Foundation
        else if (frame_type == 5)
        {
            langstroth_frames[frame_index].type = 5;
            isMaking = true;
            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 40; j++)
                {
                    langstroth_frames[frame_index].cellA[i, j] = 101;
                    langstroth_frames[frame_index].cellB[i, j] = 101;
                }
            }
            // render Foundation frame
            AddFrame(frame_index, frame_type);
        }
        Debug.Log("CREATEINITIAL frame_index: " + frame_index + "\t" + "frame_type: " + frame_type);
    }

    void SwapFrameFunc(HiveFrameWrapper hive_frame_wrapper_a, HiveFrameWrapper hive_frame_wrapper_b)
    {
        HiveFrame temp = hive_frame_wrapper_a.hive_frame;
        hive_frame_wrapper_a.hive_frame = hive_frame_wrapper_b.hive_frame;
        hive_frame_wrapper_b.hive_frame = temp;
    }

    // for adjusting frame positions in the hive; Maintaining proper beekeeping (Brood Frame Position)
    void SwapAdjustFrame(int frame_index)
    {
        int middle_frame_index = langframe_count / 2;

        for (int i = frame_index; i > 0 && i >= middle_frame_index; i--)
        {
            HiveFrameWrapper hive_frame_wrapper_a = new HiveFrameWrapper(langstroth_frames[i-1]);
            HiveFrameWrapper hive_frame_wrapper_b = new HiveFrameWrapper(langstroth_frames[i]);

            //SwapFrameFunc(i, i + 1);
            SwapFrameFunc(hive_frame_wrapper_a, hive_frame_wrapper_b);

            langstroth_frames[i-1] = hive_frame_wrapper_a.hive_frame;
            langstroth_frames[i] = hive_frame_wrapper_b.hive_frame;

            // swap rendered frames and their attributes
            GameObjectWrapper gowrapper_a = new GameObjectWrapper(frames[i - 1]);
            GameObjectWrapper gowrapper_b = new GameObjectWrapper(frames[i]);

            GameObject temp_frame = gowrapper_a.frame;
            gowrapper_a.frame = gowrapper_b.frame;
            gowrapper_b.frame = temp_frame;

            frames[i - 1] = gowrapper_a.frame;
            frames[i] = gowrapper_b.frame;

            Debug.Log("Swap Adjust Frame - swap frame positions");
                
            if (frames[i - 1] != null) Debug.Log("Frames[" + (i - 1) + "]: " + frames[i - 1].name);
            if (frames[i] != null) Debug.Log("Frames["+ i +"]: " + frames[i].name);

            // swap frames' positions
            //if (frames[i - 1] != null && frames[i] != null)
            //if (true)
            //{

            // REFERENCE
            //has_frames[i] = has_frames[selected_frame];
            //is_highlighted[i] = is_highlighted[selected_frame];
            //is_raised[i] = is_raised[selected_frame];
            //has_frames[selected_frame] = false;
            //is_highlighted[selected_frame] = false;
            //is_raised[selected_frame] = false;

            //frames[i] = frames[selected_frame];
            //Vector3 position = frame_slots[i].position;
            //position.y = frames[selected_frame].transform.position.y;
            //frames[i].transform.SetPositionAndRotation(position, frame_slots[i].rotation);

            Vector3 posForFrameA = frame_slots[i].position;
            Vector3 posForFrameB = frame_slots[i - 1].position;

            if (frames[i] != null)
            {
                posForFrameB.y = frames[i].transform.position.y;
                if (frames[i - 1] != null)
                {
                    frames[i - 1].transform.SetPositionAndRotation(posForFrameB, frame_slots[i - 1].rotation);
                }
            }

            if (frames[i - 1] != null)
            {
                posForFrameA.y = frames[i - 1].transform.position.y;
                if (frames[i] != null)
                {
                    frames[i].transform.SetPositionAndRotation(posForFrameA, frame_slots[i].rotation);
                }
            }

            //Vector3 temp_pos = frames[i - 1].transform.position;
            //frames[i - 1].transform.position = frames[i].transform.position;
            //frames[i].transform.position = temp_pos;
            //}

            //// rename frames
            //frames[i].transform.name = string.Concat(frames[i].transform.name, i);

            bool temp_has_frames = has_frames[i-1];
            has_frames[i - 1] = has_frames[i];
            has_frames[i] = temp_has_frames;

            bool temp_is_raised = is_raised[i-1];
            is_raised[i - 1] = is_raised[i];
            is_raised[i] = temp_is_raised;

            bool temp_is_highlighted = is_highlighted[i-1];
            is_highlighted[i - 1] = is_highlighted[i];
            is_highlighted[i] = temp_is_highlighted;
        }

    }

    void AddFoodFrame()
    {
        // UNCOMMENT THIS
        Debug.Log("Add FoodFrame" + isMaking);

        //Transforms S to F Frame
        if (langstroth_frames[0].type == 4)
        {
            RemoveFrame(0);
            CreateInitialFrameValues(0, 1);
        }
        /*else if(langstroth_frames[1].type == 4 && CheckFrameContinuity(1) == true){
            RemoveFrame(1);
            CreateInitialFrameValues(1,1);
        }
        else if(langstroth_frames[2].type == 4 && CheckFrameContinuity(2) == true){
            RemoveFrame(2);
            CreateInitialFrameValues(2,1);
        }
        else if(langstroth_frames[3].type == 4){
            RemoveFrame(3);
            CreateInitialFrameValues(3,1);
        }*/
        else if (langstroth_frames[4].type == 4)
        {
            RemoveFrame(4);
            CreateInitialFrameValues(4, 1);
        }
        else if (langstroth_frames[5].type == 4 && CheckFrameContinuity(5) == true)
        {
            RemoveFrame(5);
            CreateInitialFrameValues(5, 1);
        }
        else if (langstroth_frames[6].type == 4 && CheckFrameContinuity(6) == true)
        {
            RemoveFrame(6);
            CreateInitialFrameValues(6, 1);
        }
        else if (langstroth_frames[7].type == 4 && CheckFrameContinuity(7) == true)
        {
            RemoveFrame(7);
            CreateInitialFrameValues(7, 1);
        }
        else if (langstroth_frames[8].type == 4 && CheckFrameContinuity(8) == true)
        {
            RemoveFrame(8);
            CreateInitialFrameValues(8, 1);
        }
        else if (langstroth_frames[9].type == 4 && CheckFrameContinuity(9) == true)
        {
            RemoveFrame(9);
            CreateInitialFrameValues(9, 1);
        }

        //Adds a new food frame
        /*else if(langstroth_frames[0].type == 0){
            RemoveFrame(0);
            CreateInitialFrameValues(0,1);
        }
        else if(langstroth_frames[1].type == 0){
            RemoveFrame(1);
            CreateInitialFrameValues(1,1);
        }
        else if(langstroth_frames[2].type == 0){
            RemoveFrame(2);
            CreateInitialFrameValues(2,1);
        }
        else if(langstroth_frames[3].type == 0){
            RemoveFrame(3);
            CreateInitialFrameValues(3,1);
        }*/
        //Adds foundation frame for food

        else if (isMaking == false && automatic == true)
        {
            if (langstroth_frames[0].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(0, 5);
                //isMaking=true;
            }
            else if (langstroth_frames[1].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(1, 5);
                //isMaking=true;
            }
            else if (langstroth_frames[2].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(2, 5);
                //isMaking=true;
            }
            else if (langstroth_frames[3].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(3, 5);
                //isMaking=true;
            }
            else if (langstroth_frames[4].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(4, 5);
                //isMaking=true;
            }
            else if (langstroth_frames[5].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(5, 5);
                //isMaking=true;
            }
            else if (langstroth_frames[6].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(6, 5);
                //isMaking=true;
            }
            else if (langstroth_frames[7].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(7, 5);
                //isMaking=true;
            }
            else if (langstroth_frames[8].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(8, 5);
                //isMaking=true;
            }
            else if (langstroth_frames[9].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(9, 5);
                //isMaking=true;
            }
        }
    }

    void AddBroodFrame()
    {
        Debug.Log("Add BroodFrame");
        Debug.Log("isLaying: " + isLaying);
        /* TRANSFORMS Sticky TO Open Brood Frame */
            //Queen can only lay in frames in the middle of the hive
            /*
            * 1 - Impossible  
            * 2 - 4-6
            * 3 - 4-8
            * 4 - 4-10
            * 5 - 6-10      
            * 6 - 8-10
            * 7 - 9-10
            * 8 - Impossible
            * 9 - Impossible
            * 10- Impossible
            * 
            * Frames	Mid
            * 4			3
            * 5			3
            * 6			4
            * 7			4
            * 8			5
            * 9			5
            * 10		6
            * 
        */

        if (isLaying == false)
        {
            // if(langstroth_frames[0].type == 4){
            // 	Debug.Log("Transform S to O Frame");
            // 	qLoc = 10;
            // 	isLaying = true;
            // 	//CreateInitialFrameValues(0,2);
            // }
            // else... 
            if (langstroth_frames[1].type == 4 && langframe_count > 3)
            {
                Debug.Log("Transform S to O Frame");
                qLoc = 20;
                isLaying = true;
                //CreateInitialFrameValues(1,2);
            }
            else if (langstroth_frames[2].type == 4 && langframe_count > 3)
            {
                Debug.Log("Transform S to O Frame");
                qLoc = 30;
                isLaying = true;
                //CreateInitialFrameValues(2,2);
            }
            else if (langstroth_frames[3].type == 4 && langframe_count > 3)
            {
                Debug.Log("Transform S to O Frame");
                qLoc = 40;
                isLaying = true;
                //CreateInitialFrameValues(3,2);
            }
            else if (langstroth_frames[4].type == 4 && langframe_count > 5)
            {
                //CreateInitialFrameValues(4,2);
                qLoc = 50;
                isLaying = true;
            }
            else if ((langstroth_frames[5].type == 4 && langframe_count > 7) && CheckFrameContinuity(9) == true)
            {
                //CreateInitialFrameValues(5,2);
                qLoc = 60;
                isLaying = true;
            }
            else if ((langstroth_frames[6].type == 4 && langframe_count > 8) && CheckFrameContinuity(9) == true)
            {
                //CreateInitialFrameValues(6,2);
                qLoc = 70;
                isLaying = true;
            }/*
		else if(langstroth_frames[7].type == 4){
			//CreateInitialFrameValues(7,2);
			qLoc = 80;
			isLaying = true;
		}
		else if(langstroth_frames[8].type == 4){
			//CreateInitialFrameValues(8,2);
			qLoc = 90;
			isLaying = true;
		}
		else if(langstroth_frames[9].type == 4){
			//CreateInitialFrameValues(9,2);
			qLoc = 100;
			isLaying = true;
		}*/
        }
        //Adds new Foundation Frame if not already making
        else if (isMaking == false && automatic == true)
        {
            if (langstroth_frames[0].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(0, 5);
                //isMaking = true;
                SwapAdjustFrame(0);
            }
            else if (langstroth_frames[1].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(1, 5);
                //isMaking = true;
                SwapAdjustFrame(1);
            }
            else if (langstroth_frames[2].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(2, 5);
                //isMaking = true;
                SwapAdjustFrame(2);
            }
            else if (langstroth_frames[3].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(3, 5);
                //isMaking = true;
                SwapAdjustFrame(3);
            }
            else if (langstroth_frames[4].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(4, 5);
                //isMaking = true;
                SwapAdjustFrame(4);
            }
            else if (langstroth_frames[5].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(5, 5);
                //isMaking = true;
                SwapAdjustFrame(5);
            }
            else if (langstroth_frames[6].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(6, 5);
                //isMaking = true;
                SwapAdjustFrame(6);
            }
            else if (langstroth_frames[7].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(7, 5);
                //isMaking = true;
                SwapAdjustFrame(7);
            }
            else if (langstroth_frames[8].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(8, 5);
                //isMaking = true;
                SwapAdjustFrame(8);
            }
            else if (langstroth_frames[9].type == 0)
            {
                Debug.Log("Add Foundation Frame");
                CreateInitialFrameValues(9, 5);
                //isMaking = true;
                SwapAdjustFrame(9);
            }
        }
    }

    void DeleteFrame(int frame_index)
    {
        if (langstroth_frames[frame_index].type != 0)
        { 
            langframe_count -= 1;
        }

        langstroth_frames[frame_index].type = 0;
        langstroth_frames[frame_index].eggA = 0;
        langstroth_frames[frame_index].eggB = 0;
        langstroth_frames[frame_index].larvaeA = 0;
        langstroth_frames[frame_index].larvaeB = 0;
        langstroth_frames[frame_index].pupaA = 0;
        langstroth_frames[frame_index].pupaB = 0;
        langstroth_frames[frame_index].foodA = 0;
        langstroth_frames[frame_index].foodB = 0;
        langstroth_frames[frame_index].isSelected = false;

        for (int i = 0; i < 40; i++)
        {
            for (int j = 0; j < 40; j++)
            {
                langstroth_frames[frame_index].cellA[i, j] = 1000;
                langstroth_frames[frame_index].cellB[i, j] = 1000;
            }
        }

        // destroy frame
        RemoveFrame(frame_index);
    }

    void HarvestHoney()
    {
        for (int i = 1; i < FRAME_LIMIT; i++)
        {
            if (langstroth_frames[i].type == 1)
            {
                DeleteFrame(i);
            }
        }
    }

    bool CheckFrameContinuity(int frame_index)
    {
        for (int i = frame_index; i > 0; i--)
        {
            if (langstroth_frames[i].type == 0)
            {
                return false;
            }
        }
        return true;
    }

    void CheckBroodExpansionType()
    {
        //if (automatic == true)
        //{
            //End of honey season
            if ((day % 360) == 330 && automatic == true)
            {
                HarvestHoney();
            }
            QueenLayEggs();
            //Honey Season (Jan-June)
            if ((day % 360) > 180 && (day % 360) < 330)
            {
                Debug.Log("Honey Season Add Food");
                AddFoodFrame();
                AddBroodFrame();
            }
            //Brood Expansion (Jul-Dec)
            else
            {
                if (broodTotal / foodTotal >= 1)
                {
                    AddFoodFrame();
                }
                else if (broodTotal / foodTotal < 1)
                {
                    //Debug.Log(broodTotal / foodTotal);
                    AddBroodFrame();
                }
            }
        //}
    }

    // Get Total Values
    void UpdateTotalFrameQuantities()
    {
        float ft = 0f;
        int et = 0;
        int lt = 0;
        int pt = 0;

        for (int i = 0; i < FRAME_LIMIT; i++)
        {
            ft += langstroth_frames[i].foodA + langstroth_frames[i].foodB;
            et += langstroth_frames[i].eggA + langstroth_frames[i].eggB;
            lt += langstroth_frames[i].larvaeA + langstroth_frames[i].larvaeB;
            pt += langstroth_frames[i].pupaA + langstroth_frames[i].pupaB;
        }

        foodTotal = ft;
        eggTotal = et;
        larvaeTotal = lt;
        pupaTotal = pt;
        broodTotal = eggTotal + larvaeTotal + pupaTotal + workersTotal;
    }

    void InitializeWorkers()
    {
        for (int i = 0; i < 40000; i++)
        {
            workers[i] = -1;
        }
    }

    // Counts down to worker death and counts number of workers
    void UpdateWorkers()
    {
        workersTotal = 0;
        for (int i = 0; i < 40000; i++)
        {
            if (workers[i] > -1)
            {
                workers[i] -= 1;
            }
            if (workers[i] > -1)
            {
                workersTotal += 1;
            }
        }
    }

    // Places new workers into array and sets their death
    void NewWorkers(int amount)
    {
        int i = 0, j = 0;
        //System.out.println("New Workers "+workers[1]);
        for (i = 0; i < 40000 && j < amount; i++)
        {
            //System.out.println("i++");
            if (workers[i] == -1)
            {
                //Debug.Log("test random: " + (int) Random.Range(-2f, 2f));
                workers[i] = 60 + ((int) Random.Range(-3f, 3f)); // randomize each worker's lifespan by increasing or reducing its lifespan by 2 days or in between
                j += 1;
                //System.out.println("workers++");
            }
        }
    }

    // Change Frame Type and find new total values depending on cell content
    void UpdateFrameCellOrType()
    {
        if (langframe_count == 9)
        {
            isMaking = false;
        }

        //for(i=0;i<40;i++){
        //for(j=0;j<40;j++){
        /*if(frame1.cellA[i][j]>0&&frame1.cellA[i][j]<4){
            frame1.eggA++;
        }
        if(frame1.cellB[i][j]>0&&frame1.cellB[i][j]<4){
            frame1.eggB++;
        }
        if(frame1.cellA[i][j]>3&&frame1.cellA[i][j]<10){
            frame1.larvaeA++;
        }
        if(frame1.cellB[i][j]>3&&frame1.cellB[i][j]<10){
            frame1.larvaeB++;
        }*/
        //}
        //}

        //Monthly Checking
        if (day % 30 == 0)
        {
            Debug.Log("Monthy Check-up");
            CheckBroodExpansionType();
        }
        // //Egg - Sealed (Red - Green)
        for (int i = 0; i < FRAME_LIMIT; i++)
        {
            if (langstroth_frames[i].cellA[1, 1] == 10)
            {
                DeleteFrame(i);
                CreateInitialFrameValues(i, 3);
                CheckBroodExpansionType();
                UpdateTotalFrameQuantities();
            }
        }
        // //Pupa to Sticky - (Green - Blue)
        for (int i = 0; i < FRAME_LIMIT; i++)
        {
            if (langstroth_frames[i].cellA[1, 1] == 22)
            {
                DeleteFrame(i);
                CreateInitialFrameValues(i, 4);
                CheckBroodExpansionType();
                UpdateTotalFrameQuantities();
                NewWorkers(3200);
                //workersTotal=workersTotal+3200;
            }
        }
        // //Foundation to Sticky (Gold-Blue)
        for (int i = 0; i < FRAME_LIMIT; i++)
        {
            if (langstroth_frames[i].cellA[1, 1] == 114)
            {
                if (CheckFrameContinuity(i) == false)
                {
                    DeleteFrame(i);
                    CreateInitialFrameValues(i, 5);
                }
                else
                {
                    DeleteFrame(i);
                    CreateInitialFrameValues(i, 4);
                    CheckBroodExpansionType();
                    UpdateTotalFrameQuantities();
                    isMaking = false;
                }
            }
        }

        //Queen Laying Eggs
        if (qLoc == 12)
        {   
            if (langstroth_frames[0].type != 0)
            {
                RemoveFrame(0);
            }
            CreateInitialFrameValues(0, 2);
            qLoc += 1;
            isLaying = false;
        }
        else if (qLoc == 22)
        {
            if (langstroth_frames[1].type != 0)
            {
                RemoveFrame(1);
            }
            CreateInitialFrameValues(1, 2);
            qLoc += 1;
            isLaying = false;
        }
        else if (qLoc == 32)
        {
            if (langstroth_frames[2].type != 0)
            {
                RemoveFrame(2);
            }
            CreateInitialFrameValues(2, 2);
            qLoc += 1;
            isLaying = false;
        }
        else if (qLoc == 42)
        {
            if (langstroth_frames[3].type != 0)
            {
                RemoveFrame(3);
            }
            CreateInitialFrameValues(3, 2);
            qLoc += 1;
            isLaying = false;
        }
        else if (qLoc == 52)
        {
            if (langstroth_frames[4].type != 0)
            {
                RemoveFrame(4);
            }
            CreateInitialFrameValues(4, 2);
            qLoc += 1;
            isLaying = false;
        }
        else if (qLoc == 62)
        {
            if (langstroth_frames[5].type != 0)
            {
                RemoveFrame(5);
            }
            CreateInitialFrameValues(5, 2);
            qLoc += 1;
            isLaying = false;
        }
        else if (qLoc == 72)
        {
            if (langstroth_frames[6].type != 0)
            {
                RemoveFrame(6);
            }
            CreateInitialFrameValues(6, 2);
            qLoc += 1;
            isLaying = false;
        }
        else if (qLoc == 82)
        {
            if (langstroth_frames[7].type != 0)
            {
                RemoveFrame(7);
            }
            CreateInitialFrameValues(7, 2);
            qLoc += 1;
            isLaying = false;
        }
        else if (qLoc == 92)
        {
            if (langstroth_frames[8].type != 0)
            {
                RemoveFrame(8);
            }
            CreateInitialFrameValues(8, 2);
            qLoc += 1;
            isLaying = false;
        }
        else if (qLoc == 102)
        {
            if (langstroth_frames[9].type != 0)
            {
                RemoveFrame(9);
            }
            CreateInitialFrameValues(9, 2);
            qLoc += 1;
            isLaying = false;
        }
    }

    public static void ChangeInterval(float _sleepInterval)
    {
        sleepInterval = _sleepInterval;
    }

    public void ChangeSleepInterval(float _sleepInterval)
    {
        sleepInterval = _sleepInterval;
        GameState.ChangeCurrentInterval(_sleepInterval);
    }

    public void UpdateSpeed(float timeScale)
    {
        Time.timeScale = timeScale;
    }
}
