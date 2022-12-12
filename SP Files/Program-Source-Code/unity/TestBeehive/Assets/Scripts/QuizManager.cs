using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;
using System.IO;
using TMPro;

public class QuizManager : MonoBehaviour
{
    public Transform[] frame_slots;
    public GameObject[] frame_models;
    public HiveFrame[] langstroth_frames = new HiveFrame[10];
    public GameObject main_cam_ref_point;
    public CanvasGroup main_canvas;
    public Text[] data_monitors;
    public Canvas quiz_canvas;

    public static bool automatic = false;
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

    // Quiz variables
    private bool isQuizReset = false;
    private int current_set = -1;
    private int current_item = -1;
    private string[] answers = new string[10];
    private string[,] choices_set1 = new string[10, 4];
    private string[,] choices_set2 = new string[10, 4];
    private string[,] choices_set3 = new string[10, 4];
    private int[] scores = new int[3];

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
    //private int timer = 0;
    private int day = 0;
    private bool isInitialSetup = false;
    public static float sleepInterval = 1f;

    private StreamWriter sw;

    public static bool isReset = false;
    public static bool hasRead = false;
    
    // Start is called before the first frame update
    void Start()
    {
        if (isReset)
        {
            Reset();
        }
        isReset = false;

        //placeholder_frame = new GameObject();
    }

    void initializeHive()
    {
        for (int i = 0; i < 10; i++)
        {
            has_frames[i] = false;
            is_raised[i] = false;
            is_highlighted[i] = false;

            Destroy(frames[i]);
            frames[i] = placeholder_frame;
        }

        selected_frame = -1;
        prev_selected_frame = selected_frame;
        frame_count = 0;
        langframe_count = 0;
        current_frame_type = 0;
        current_back_most_frame_slot = 0;
    }

    public void LoadSet(int set_number)
    {
        if (set_number != current_set || isQuizReset)
        {
            initializeHive();
            if (set_number == 1)
            {
                current_set = set_number;
                scores[current_set - 1] = 0;
                main_canvas.gameObject.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = "Score: " + scores[current_set - 1] + "/10";

                // initialize string values
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        choices_set1[i, j] = "null";
                    }
                }

                // load frames multiple choices
                AddFrame(0, 1);
                choices_set1[0, 3] = "Food";
                choices_set1[0, 1] = "Foundation";
                choices_set1[0, 0] = "Open Brood";
                choices_set1[0, 2] = "Sealed Brood";

                AddFrame(1, 3);
                choices_set1[1, 1] = "Sealed Brood";
                choices_set1[1, 3] = "Open Brood";
                choices_set1[1, 2] = "Food";
                choices_set1[1, 0] = "Sticky";

                AddFrame(2, 3);
                choices_set1[2, 0] = "Sealed Brood";
                choices_set1[2, 3] = "Food";
                choices_set1[2, 2] = "Open Brood";
                choices_set1[2, 1] = "Sticky";

                AddFrame(3, 3);
                choices_set1[3, 3] = "Sealed Brood";
                choices_set1[3, 1] = "Food";
                choices_set1[3, 0] = "Sticky";
                choices_set1[3, 2] = "Open Brood";

                AddFrame(4, 2);
                choices_set1[4, 1] = "Open Brood";
                choices_set1[4, 3] = "Foundation";
                choices_set1[4, 2] = "Sticky";
                choices_set1[4, 0] = "Sealed Brood";

                AddFrame(5, 2);
                choices_set1[5, 3] = "Open Brood";
                choices_set1[5, 0] = "Sticky";
                choices_set1[5, 1] = "Food";
                choices_set1[5, 2] = "Sealed Brood";

                AddFrame(6, 3);
                choices_set1[6, 3] = "Sealed Brood";
                choices_set1[6, 0] = "Foundation";
                choices_set1[6, 2] = "Open Brood";
                choices_set1[6, 1] = "Sticky";

                AddFrame(7, 3);
                choices_set1[7, 0] = "Sealed Brood";
                choices_set1[7, 3] = "Food";
                choices_set1[7, 2] = "Sticky";
                choices_set1[7, 1] = "Sealed Brood";

                AddFrame(8, 3);
                choices_set1[8, 2] = "Sealed Brood";
                choices_set1[8, 1] = "Food";
                choices_set1[8, 3] = "Foundation";
                choices_set1[8, 0] = "Sticky";

                AddFrame(9, 1);
                choices_set1[9, 0] = "Food";
                choices_set1[9, 1] = "Foundation";
                choices_set1[9, 3] = "Open Brood";
                choices_set1[9, 2] = "Sticky";

                answers[0] = "Food";
                answers[1] = "Sealed Brood";
                answers[2] = "Sealed Brood";
                answers[3] = "Sealed Brood";
                answers[4] = "Open Brood";
                answers[5] = "Open Brood";
                answers[6] = "Sealed Brood";
                answers[7] = "Sealed Brood";
                answers[8] = "Sealed Brood";
                answers[9] = "Food";
            }
            else if (set_number == 2)
            {
                current_set = set_number;
                scores[current_set - 1] = 0;
                main_canvas.gameObject.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = "Score: " + scores[current_set - 1] + "/10";

                // initialize string values
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        choices_set1[i, j] = "null";
                    }
                }

                // load frames multiple choices
                AddFrame(0, 2);
                choices_set1[0, 3] = "Open Brood";
                choices_set1[0, 1] = "Foundation";
                choices_set1[0, 0] = "Food";
                choices_set1[0, 2] = "Sticky";

                AddFrame(1, 3);
                choices_set1[1, 1] = "Sealed Brood";
                choices_set1[1, 3] = "Open Brood";
                choices_set1[1, 2] = "Food";
                choices_set1[1, 0] = "Foundation";

                AddFrame(2, 2);
                choices_set1[2, 0] = "Open Brood";
                choices_set1[2, 3] = "Food";
                choices_set1[2, 2] = "Sealed Brood";
                choices_set1[2, 1] = "Foundation";

                AddFrame(3, 3);
                choices_set1[3, 3] = "Sealed Brood";
                choices_set1[3, 1] = "Food";
                choices_set1[3, 0] = "Sticky";
                choices_set1[3, 2] = "Open Brood";

                AddFrame(4, 1);
                choices_set1[4, 1] = "Food";
                choices_set1[4, 3] = "Foundation";
                choices_set1[4, 2] = "Sticky";
                choices_set1[4, 0] = "Sealed Brood";

                AddFrame(5, 1);
                choices_set1[5, 3] = "Food";
                choices_set1[5, 0] = "Sticky";
                choices_set1[5, 1] = "Open Brood";
                choices_set1[5, 2] = "Sealed Brood";

                AddFrame(6, 2);
                choices_set1[6, 3] = "Open Brood";
                choices_set1[6, 0] = "Foundation";
                choices_set1[6, 2] = "Food";
                choices_set1[6, 1] = "Sticky";

                AddFrame(7, 4);
                choices_set1[7, 0] = "Sticky";
                choices_set1[7, 3] = "Food";
                choices_set1[7, 2] = "Foundation";
                choices_set1[7, 1] = "Sealed Brood";

                AddFrame(8, 4);
                choices_set1[8, 2] = "Sticky";
                choices_set1[8, 1] = "Food";
                choices_set1[8, 3] = "Foundation";
                choices_set1[8, 0] = "Sealed Brood";

                AddFrame(9, 2);
                choices_set1[9, 0] = "Open Brood";
                choices_set1[9, 1] = "Foundation";
                choices_set1[9, 3] = "Food";
                choices_set1[9, 2] = "Sticky";

                answers[0] = "Open Brood";
                answers[1] = "Sealed Brood";
                answers[2] = "Open Brood";
                answers[3] = "Sealed Brood";
                answers[4] = "Food";
                answers[5] = "Food";
                answers[6] = "Open Brood";
                answers[7] = "Sticky";
                answers[8] = "Sticky";
                answers[9] = "Open Brood";
            }
            else if (set_number == 3)
            {
                current_set = set_number;
                scores[current_set - 1] = 0;
                main_canvas.gameObject.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = "Score: " + scores[current_set - 1] + "/10";

                // initialize string values
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        choices_set1[i, j] = "null";
                    }
                }

                // load frames multiple choices
                AddFrame(0, 4);
                choices_set1[0, 3] = "Sticky";
                choices_set1[0, 1] = "Food";
                choices_set1[0, 0] = "Open Brood";
                choices_set1[0, 2] = "Foundation";

                AddFrame(1, 2);
                choices_set1[1, 1] = "Open Brood";
                choices_set1[1, 3] = "Sealed Brood";
                choices_set1[1, 2] = "Foundation";
                choices_set1[1, 0] = "Food";

                AddFrame(2, 2);
                choices_set1[2, 0] = "Open Brood";
                choices_set1[2, 3] = "Foundation";
                choices_set1[2, 2] = "Sealed Brood";
                choices_set1[2, 1] = "Sticky";

                AddFrame(3, 1);
                choices_set1[3, 3] = "Food";
                choices_set1[3, 1] = "Foundation";
                choices_set1[3, 0] = "Sealed Brood";
                choices_set1[3, 2] = "Open Brood";

                AddFrame(4, 3);
                choices_set1[4, 1] = "Sealed Brood";
                choices_set1[4, 3] = "Food";
                choices_set1[4, 2] = "Sticky";
                choices_set1[4, 0] = "Open Brood";

                AddFrame(5, 3);
                choices_set1[5, 3] = "Sealed Brood";
                choices_set1[5, 0] = "Foundation";
                choices_set1[5, 1] = "Food";
                choices_set1[5, 2] = "Sticky";

                AddFrame(6, 1);
                choices_set1[6, 3] = "Food";
                choices_set1[6, 0] = "Foundation";
                choices_set1[6, 2] = "Open Brood";
                choices_set1[6, 1] = "Sticky";

                AddFrame(7, 2);
                choices_set1[7, 0] = "Open Brood";
                choices_set1[7, 3] = "Food";
                choices_set1[7, 2] = "Sticky";
                choices_set1[7, 1] = "Sealed Brood";

                AddFrame(8, 2);
                choices_set1[8, 2] = "Open Brood";
                choices_set1[8, 1] = "Food";
                choices_set1[8, 3] = "Foundation";
                choices_set1[8, 0] = "Sticky";

                AddFrame(9, 4);
                choices_set1[9, 0] = "Sticky";
                choices_set1[9, 1] = "Foundation";
                choices_set1[9, 3] = "Open Brood";
                choices_set1[9, 2] = "Sealed Brood";

                answers[0] = "Sticky";
                answers[1] = "Open Brood";
                answers[2] = "Open Brood";
                answers[3] = "Food";
                answers[4] = "Sealed Brood";
                answers[5] = "Sealed Brood";
                answers[6] = "Food";
                answers[7] = "Open Brood";
                answers[8] = "Open Brood";
                answers[9] = "Sticky";
            }
        }
    }

    public void ResetQuiz()
    {
        isQuizReset = true;
        LoadSet(current_set);
        isQuizReset = false;
    }

    public void ShowChoices()
    {
        current_item = selected_frame;
        Debug.Log(current_item);

        quiz_canvas.gameObject.transform.Find("ChoiceA").Find("Text").GetComponent<Text>().text = choices_set1[selected_frame, 0];
        quiz_canvas.gameObject.transform.Find("ChoiceB").Find("Text").GetComponent<Text>().text = choices_set1[selected_frame, 1];
        quiz_canvas.gameObject.transform.Find("ChoiceC").Find("Text").GetComponent<Text>().text = choices_set1[selected_frame, 2];
        quiz_canvas.gameObject.transform.Find("ChoiceD").Find("Text").GetComponent<Text>().text = choices_set1[selected_frame, 3];

        quiz_canvas.gameObject.SetActive(true);
    }

    public void HideChoices()
    {
        quiz_canvas.gameObject.SetActive(false);
    }

    public void CheckChoice(string choiceButton)
    {
        if (quiz_canvas.gameObject.transform.Find(choiceButton).Find("Text").GetComponent<Text>().text == answers[current_item])
        {
            scores[current_set - 1] += 1;
            main_canvas.gameObject.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = "Score: " + scores[current_set - 1] + "/10";

            // highlight selected frame to GREEN
            Renderer frame_renderer = frames[current_item].GetComponent<Renderer>();
            Color col = frame_renderer.material.GetColor("_Color");
            frame_renderer.material.SetColor("_Color", new Color(0.1f, col.g + 0.1f, 0.1f, 1f));
        }
        else
        {
            // highlight selected frame to RED
            Renderer frame_renderer = frames[current_item].GetComponent<Renderer>();
            Color col = frame_renderer.material.GetColor("_Color");
            frame_renderer.material.SetColor("_Color", new Color(col.r + 0.25f, 0.1f, 0.1f, 1f));
        }

        quiz_canvas.gameObject.SetActive(false);
        // enable frame label
        Debug.Log(frames[current_item].name);
        Debug.Log(GetFrameTypeName(current_item));
        frames[current_item].gameObject.transform.Find(GetFrameTypeName(current_item)).gameObject.SetActive(true);
        frames[current_item].gameObject.GetComponent<BoxCollider>().enabled = false;
        LowerFrame(current_item);
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
                        Debug.Log("i trans name: " + frames[i].gameObject.transform.name + " | hit trans name: " + hit.transform.name);
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
    }

    public void AddFrame()
    {
        if (frame_count < 10 && !has_frames[current_back_most_frame_slot])
        {
            frames[current_back_most_frame_slot] = Instantiate(frame_models[current_frame_type], parent);
            frames[current_back_most_frame_slot].gameObject.GetComponent<BoxCollider>().enabled = true;
            frames[current_back_most_frame_slot].GetComponent<Transform>().SetPositionAndRotation(frame_slots[current_back_most_frame_slot].position, frame_slots[current_back_most_frame_slot].rotation);

            // rename frame; make new name unique
            frames[current_back_most_frame_slot].transform.name = string.Concat(frames[current_back_most_frame_slot].transform.name, current_back_most_frame_slot);

            has_frames[current_back_most_frame_slot] = true;
            SetCurrentBackmostFrameSlot();
            frame_count += 1;


            //Debug.Log("ADD BUTTON CLICKED AND FRAME ADDED AT " + current_back_most_frame_slot + "!");
        }
        else
        {
            return;
        }
    }

    public string GetFrameTypeName(int index)
    {
        string name = frames[index].transform.name;
        string substring = name.Substring(0, name.Length - 8);

        Debug.Log(substring);
        return substring;
    }

    public void AddFrame(int index, int frame_type)
    {
        if (frame_count < 10 && !has_frames[index])
        { 
            frames[index] = Instantiate(frame_models[frame_type], parent);
            //frames[index].gameObject.GetComponent<BoxCollider>().enabled = true;
            frames[index].GetComponent<Transform>().SetPositionAndRotation(frame_slots[index].position, frame_slots[index].rotation);

            // rename frame; make new name unique for raycast detection
            frames[index].transform.name = string.Concat(frames[index].transform.name, index);

            // disable frame label
            frames[index].gameObject.transform.Find(GetFrameTypeName(index)).gameObject.SetActive(false);

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
            //Debug.Log("////////////////////////////////////////////");
            Destroy(frames[index]);
            //Debug.Log("Deleted frames[" + index + "]: " + frames[index]);
            //Debug.Log("////////////////////////////////////////////");
            has_frames[index] = false;
            is_raised[index] = false;
            is_highlighted[index] = false;
            frame_count -= 1;
            SetCurrentBackmostFrameSlot();
            Debug.Log("(" + index + ") FRAME REMOVED!");
        }
    }

    public void RemoveSelectedFrame()
    {
        if (selected_frame > -1 && has_frames[selected_frame])
        {
            Debug.Log("frame_count: " + frame_count);

            Destroy(frames[selected_frame]);
            has_frames[selected_frame] = false;
            is_raised[selected_frame] = false;
            is_highlighted[selected_frame] = false;
            frame_count -= 1;
            SetCurrentBackmostFrameSlot();
            Debug.Log("REMOVE BUTTON CLICKED AND FRAME REMOVED!");
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
    
    public static void ChangeInterval(float _sleepInterval)
    {
        sleepInterval = _sleepInterval;
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return (results.Count > 0) ? true : false;
    }
}
