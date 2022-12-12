using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;

public class HiveGraph : MonoBehaviour
{
    public static HiveGraph instance;

    public Sprite dotSprite;
    private RectTransform graphCanvas;
    private RectTransform windowGraph;
    private RectTransform graphContainer;
    private RectTransform labelXAxis;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;
    private List<List<GameObject>> gameObjectList;
    private List<List<IGraphVisualObject>> graphVisualObjectList;
    private GameObject toolTipGameObject;
    private List<int> valueList;
    private IGraphVisual graphVisual;
    private float scaleFactor;
    private int count = 0;

    private bool hasYLabels;
    private bool isMinimized;
    public static bool noConnect = false;

    private int number = 0;

    private void Awake()
    {
        instance = this;
        graphCanvas = transform.parent.GetComponent<RectTransform>();
        windowGraph = transform.GetComponent<RectTransform>();
        graphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
        labelXAxis = graphContainer.Find("LabelXAxis").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("LabelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("LabelTemplateY").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("DashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("DashTemplateY").GetComponent<RectTransform>();
        toolTipGameObject = graphContainer.Find("Tooltip").gameObject;
        hasYLabels = false;
        isMinimized = false;

        gameObjectList = new List<List<GameObject>>();
        gameObjectList.Add(new List<GameObject>());

        graphVisualObjectList = new List<List<IGraphVisualObject>>();
        graphVisualObjectList.Add(new List<IGraphVisualObject>());

        count = 1;

        //scaleFactor = graphCanvas.GetComponent<CanvasScaler>().scaleFactor;
        //Debug.Log(scaleFactor);
        //Debug.Log(graphContainer.localScale);
        //Debug.Log(graphContainer.parent.localScale);
        //Debug.Log(windowGraph.parent.localScale);
        //Debug.Log(graphCanvas.localScale);
        ////graphContainer.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        ////windowGraph.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        ////graphCanvas.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        //Debug.Log("-----");
        //Debug.Log(windowGraph.parent.localScale);
        //Debug.Log(graphContainer.parent.localScale);
        //Debug.Log(graphContainer.localScale);

        //windowGraph.position = new Vector3(30f, 30f, 0);
        //transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        valueList = new List<int>();
        graphVisual = new LineGraphVisual(graphContainer, dotSprite, new Color(0.9f, 0.9f, 0f, 1f));

        //ShowGraph(valueList, graphVisual, (int _i) => "Day " + (_i + 1), (float _f) => "" + Mathf.RoundToInt(_f));
    }

    public void AppendToValueList(int data, int index)
    {
        if (index > count - 1)
        {
            count += 1;
            gameObjectList.Add(new List<GameObject>());
            graphVisualObjectList.Add(new List<IGraphVisualObject>());
            valueList = new List<int>();

            noConnect = true;

            graphVisual = new LineGraphVisual(graphContainer, dotSprite, new Color(0.9f, 0f, 0f, 1f));

            Debug.Log("value list COUNT: " + valueList.Count);
        }

        //IGraphVisual graphVisual = new LineGraphVisual(graphContainer, dotSprite, new Color(1, 1, 0, 0.75f));
        valueList.Add(data);
        number += 1;

        //if (number == 60)
        //{
        //    for (int i = 0; i < valueList.Count; i++)
        //    {
        //        valueList.RemoveAt(i);
        //    }

        //    foreach (GameObject gameObject in gameObjectList)
        //    {
        //        Destroy(gameObject);
        //    }
        //    gameObjectList.Clear();

        //    foreach (IGraphVisualObject graphVisualObject in graphVisualObjectList)
        //    {
        //        graphVisualObject.Cleanup();
        //    }
        //    graphVisualObjectList.Clear();
        //}

        //ShowGraph(index, valueList, graphVisual, (int _i) => "Day " + (_i + 1), (float _f) => "" + Mathf.RoundToInt(_f));
        UpdateGraph(index, valueList, graphVisual, (int _i) => "" + (_i), (float _f) => Mathf.RoundToInt(_f) + " K");
        hasYLabels = true;

        //Debug.Log("DATA ADDED: " + data);
    }

    public static void ShowTooltip_Static(string tooltipText, Vector2 anchoredPosition)
    {
        instance.ShowTooltip(tooltipText, anchoredPosition);
    }

    public static void HideTooltip_Static()
    {
        instance.HideTooltip();
    }

    private void ShowTooltip(String tooltipText, Vector2 anchoredPosition)
    {
        Text tooltipUIText = toolTipGameObject.transform.Find("Text").GetComponent<Text>();
        tooltipUIText.text = tooltipText;
        float textPaddingSize = 4f;
        Vector2 backgroundSize = new Vector2(
            tooltipUIText.preferredWidth + (textPaddingSize * 2f),
            tooltipUIText.preferredHeight + (textPaddingSize * 2f)
        );

        toolTipGameObject.transform.Find("Background").GetComponent<RectTransform>().sizeDelta = backgroundSize;

        toolTipGameObject.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;

        toolTipGameObject.transform.SetAsLastSibling();
        toolTipGameObject.SetActive(true);
    }

    private void HideTooltip()
    {
        toolTipGameObject.SetActive(false);
    }

    public void ToggleVisibility()
    {
        //transform.gameObject.SetActive(false);
        windowGraph.gameObject.SetActive(!windowGraph.gameObject.activeSelf);
    }

    private void ShowGraph(int index, List<int> valueList, IGraphVisual graphVisual, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
    {
        // Func is like an anonymous function that is passed as one of parameters in ShowGraph
        if (getAxisLabelX == null)
        {
            // assign default value
            getAxisLabelX = delegate (int _i) { return _i.ToString(); };
        }
        if (getAxisLabelY == null)
        {
            getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
        }

        foreach (GameObject gameObject in gameObjectList[0])
        {
            Destroy(gameObject);
        }
        gameObjectList.Clear();

        foreach (IGraphVisualObject graphVisualObject in graphVisualObjectList[0])
        {
            graphVisualObject.Cleanup();
        }
        graphVisualObjectList.Clear();

        float graphHeight = graphContainer.sizeDelta.y;

        float yMaximum = valueList[0];
        float yMinimum = valueList[0];

        foreach (int value in valueList)
        {
            if (value > yMaximum)
            {
                yMaximum = value;
            }
            if (value < yMinimum)
            {
                yMinimum = value;
            }
        }
        // STATIC Y Max and Min
        yMaximum = 75000f;
        yMinimum = 0f;

        // DYNAMIC Y Max and Min
        // yMaximum = yMaximum + ((yMaximum - yMinimum) * 0.2f);
        // yMinimum = yMinimum - ((yMaximum - yMinimum) * 0.2f);

        float xSize = 1.5f;
        
        for (int i = 0; i < valueList.Count; i++)
        {
            // float xPosition = (i + xSize) * xSize;
            float xPosition = (i + 1) * xSize;

            // STATIC Y-axis
            float yPosition = ((valueList[i]) / (yMaximum)) * graphHeight;
            // DYNAMIC Y-axis
            //float yPosition = ((valueList[i] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;

            string tooltipText = getAxisLabelY(valueList[i]);
            graphVisualObjectList[0].Add(graphVisual.CreateGraphVisualObject(new Vector2(xPosition, yPosition), xSize, tooltipText));

            if (i == 0 || (i + 1) % 30 == 0)
            {
                RectTransform labelX = Instantiate(labelTemplateX);
                labelX.SetParent(graphContainer);
                labelX.gameObject.SetActive(true);
                labelX.anchoredPosition = new Vector2(xPosition, -10f);
                labelX.GetComponent<Text>().text = getAxisLabelX(i);

                gameObjectList[0].Add(labelX.gameObject);
            }

            if ((i + 1) % 30 == 0)
            {
                RectTransform dashX = Instantiate(dashTemplateY);
                dashX.SetParent(graphContainer);
                dashX.gameObject.SetActive(true);
                dashX.anchoredPosition = new Vector2(xPosition, -10f);
                dashX.transform.SetSiblingIndex(1);

                gameObjectList[0].Add(dashX.gameObject);
            }
        }
       
        int separatorCount = 5;

        for (int i = 0; i <= separatorCount; i++)
        {
            float normalizedValue = i * 1f / separatorCount;

            //if ((i + 1) % 1 == 0)
            //{
                RectTransform labelY = Instantiate(labelTemplateY);
                labelY.SetParent(graphContainer);
                labelY.gameObject.SetActive(true);
                labelY.anchoredPosition = new Vector2(-10f, normalizedValue * graphHeight);

                // STATIC Y-axis labels 
                labelY.GetComponent<Text>().text = getAxisLabelY(normalizedValue * yMaximum);
                // DYNAMIC Y-axis labels
                //labelY.GetComponent<Text>().text = getAxisLabelY(yMinimum + (normalizedValue * (yMaximum - yMinimum)));

                gameObjectList[0].Add(labelY.gameObject);
            //}

            //if ((i + 1) % 1 == 0)
            //{
                RectTransform dashY = Instantiate(dashTemplateX);
                dashY.SetParent(graphContainer);
                dashY.gameObject.SetActive(true);
                dashY.anchoredPosition = new Vector2(-4f, normalizedValue * graphHeight);
                dashY.transform.SetSiblingIndex(1);

                gameObjectList[0].Add(dashY.gameObject);
            //}
        }
    }

    private void UpdateGraph(int index, List<int> valueList, IGraphVisual graphVisual, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
    {
        // Func is like an anonymous function that is passed as one of parameters in ShowGraph
        if (getAxisLabelX == null)
        {
            // assign default value
            getAxisLabelX = delegate (int _i) { return _i.ToString(); };
        }
        if (getAxisLabelY == null)
        {
            getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
        }

        //foreach (GameObject gameObject in gameObjectList)
        //{
        //    Destroy(gameObject);
        //}
        //gameObjectList.Clear();

        //foreach (IGraphVisualObject graphVisualObject in graphVisualObjectList)
        //{
        //    graphVisualObject.Cleanup();
        //}
        //graphVisualObjectList.Clear();

        Vector2 sizeDelta = graphContainer.sizeDelta;
        float graphHeight = sizeDelta.y;
        float graphWidth = sizeDelta.x;

        float yMaximum = valueList[0];
        float yMinimum = valueList[0];

        // MOVE GRAPH
        //windowGraph.position = new Vector3(90f, 60f, 0);

        // STATIC Y Max and Min
        yMaximum = 60000f;
        yMinimum = 0f;

        // DYNAMIC Y Max and Min
        //foreach (int value in valueList)
        //{
        //    if (value > yMaximum)
        //    {
        //        yMaximum = value;
        //    }
        //    if (value < yMinimum)
        //    {
        //        yMinimum = value;
        //    }
        //}

        // DYNAMIC Y Max and Min
        // yMaximum = yMaximum + ((yMaximum - yMinimum) * 0.2f);
        // yMinimum = yMinimum - ((yMaximum - yMinimum) * 0.2f);

        float xSize = 1.7f;

        int i = valueList.Count - 1;
        int days = valueList.Count;

        //for (int i = 0; i < valueList.Count; i++)
        //{
        //    // float xPosition = (i + xSize) * xSize;
        //    float xPosition = (i + 1) * xSize;

        //    // STATIC Y-axis
        //    float yPosition = ((valueList[i]) / (yMaximum)) * graphHeight;
        //    // DYNAMIC Y-axis
        //    //float yPosition = ((valueList[i] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;

        //    string tooltipText = getAxisLabelY(valueList[i]);
        //    graphVisualObjectList.Add(graphVisual.CreateGraphVisualObject(new Vector2(xPosition, yPosition), xSize, tooltipText));

        //    if (i == 0 || (i + 1) % 30 == 0)
        //    {
        //        RectTransform labelX = Instantiate(labelTemplateX);
        //        labelX.SetParent(graphContainer);
        //        labelX.gameObject.SetActive(true);
        //        labelX.anchoredPosition = new Vector2(xPosition, -10f);
        //        labelX.GetComponent<Text>().text = getAxisLabelX(i);

        //        gameObjectList.Add(labelX.gameObject);
        //    }

        //    if ((i + 1) % 30 == 0)
        //    {
        //        RectTransform dashX = Instantiate(dashTemplateY);
        //        dashX.SetParent(graphContainer);
        //        dashX.gameObject.SetActive(true);
        //        dashX.anchoredPosition = new Vector2(xPosition, -10f);
        //        dashX.transform.SetSiblingIndex(1);

        //        gameObjectList.Add(dashX.gameObject);
        //    }

        // float xPosition = (i + xSize) * xSize;
        float xPosition = (i + 1) * xSize;

        // STATIC Y-axis
        float yPosition = ((valueList[i]) / (yMaximum)) * graphHeight;
        // DYNAMIC Y-axis
        //float yPosition = ((valueList[i] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;

        string tooltipText = getAxisLabelY(valueList[i]);
        graphVisualObjectList[index].Add(graphVisual.CreateGraphVisualObject(new Vector2(xPosition, yPosition), xSize, tooltipText));

        if (days % 30 == 0)
        {
            RectTransform labelX = Instantiate(labelTemplateX);
            labelX.SetParent(graphContainer);
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(xPosition, -5f);
            labelX.GetComponent<Text>().text = getAxisLabelX(days);
            labelX.GetComponent<Text>().fontSize = 14;

            gameObjectList[index].Add(labelX.gameObject);

            RectTransform dashX = Instantiate(dashTemplateY);
            dashX.SetParent(graphContainer);
            dashX.gameObject.SetActive(true);
            //dashX.offsetMin = new Vector2(0f, 0f);
            //dashX.offsetMax = new Vector2(0f, 0f);
            dashX.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, graphHeight * graphContainer.transform.localScale.y);
            dashX.anchoredPosition = new Vector2(xPosition, 0f);
            dashX.transform.SetSiblingIndex(1);

            gameObjectList[index].Add(dashX.gameObject);
        }
        //}

        int separatorCount = 5;

        for (i = 0; i <= separatorCount && !hasYLabels; i++)
        {
            float normalizedValue = i * 1f / separatorCount;
            
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer);
            labelY.gameObject.SetActive(true);
            labelY.anchoredPosition = new Vector2(-5f, normalizedValue * graphHeight);

            // STATIC Y-axis labels 
            labelY.GetComponent<Text>().text = getAxisLabelY((normalizedValue * yMaximum) / 1000);
            // DYNAMIC Y-axis labels
            //labelY.GetComponent<Text>().text = getAxisLabelY(yMinimum + (normalizedValue * (yMaximum - yMinimum)));

            labelY.GetComponent<Text>().fontSize = 14;

            gameObjectList[index].Add(labelY.gameObject);

            RectTransform dashY = Instantiate(dashTemplateX);
            dashY.SetParent(graphContainer);
            dashY.gameObject.SetActive(true);
            //dashY.offsetMin = new Vector2(0f, 0f);
            //dashY.offsetMax = new Vector2(0f, 0f);
            dashY.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, graphWidth * graphContainer.transform.localScale.x);
            dashY.anchoredPosition = new Vector2(0f, normalizedValue * graphHeight);
            dashY.transform.SetSiblingIndex(1);

            gameObjectList[index].Add(dashY.gameObject);
        }
    }

    // Interface definition for showing visual for a data point
    private interface IGraphVisual
    {
        IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float graphPositionWidth, string tooltipText);
    }

    // Represents a single visual Object in the graph
    private interface IGraphVisualObject
    {
        void SetGraphVisualObjectInfo(Vector2 graphPosition, float graphPositionWidth, string tooltipText);
        void Cleanup();
    }

    private class LineGraphVisual : IGraphVisual
    {
        private RectTransform graphContainer;
        private LineGraphVisualObject lastLineGraphVisualObject;
        private Sprite dotSprite;
        private Color dotConnectionColor;

        public LineGraphVisual(RectTransform graphContainer, Sprite dotSprite, Color dotConnectionColor)
        {
            this.graphContainer = graphContainer;
            this.lastLineGraphVisualObject = null;
            this.dotSprite = dotSprite;
            this.dotConnectionColor = dotConnectionColor;
        }

        private static float GetAngleFromVectorFloat(Vector2 dir)
        {
            float difference = (dir.y / dir.x);
            float angleRot = Mathf.Atan(difference) * 180 / Mathf.PI;

            return angleRot;
        }

        public IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float graphPositionWidth, string tooltipText)
        {
            // draw data point
            GameObject dotGameObject = CreateDot(graphPosition);
            
            GameObject dotConnectionGameObject = null;

            LineGraphVisualObject lineGraphVisualObject;

            if (noConnect)
            {
                lineGraphVisualObject = new LineGraphVisualObject(dotGameObject, dotConnectionGameObject, null);
                lineGraphVisualObject.SetGraphVisualObjectInfo(graphPosition, graphPositionWidth, tooltipText);
            }
            else
            {
                if (lastLineGraphVisualObject != null)
                {
                    dotConnectionGameObject = CreateDotConnection(lastLineGraphVisualObject.GetGraphPosition(), dotGameObject.GetComponent<RectTransform>().anchoredPosition);
                    dotConnectionGameObject.transform.SetSiblingIndex(lastLineGraphVisualObject.GetDotGameObject().transform.GetSiblingIndex());
                }

                lineGraphVisualObject = new LineGraphVisualObject(dotGameObject, dotConnectionGameObject, lastLineGraphVisualObject);
                lineGraphVisualObject.SetGraphVisualObjectInfo(graphPosition, graphPositionWidth, tooltipText);
            }
            noConnect = false;

            lastLineGraphVisualObject = lineGraphVisualObject;


            return lineGraphVisualObject;
        }

        private GameObject CreateDot(Vector2 anchoredPosition)
        {
            GameObject gameObject = new GameObject("dataPoint", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().sprite = dotSprite;

            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(0, 0);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            // Add Data Point icon to gameObject
            Button_UI dotButtonUI = gameObject.AddComponent<Button_UI>();

            return gameObject;
        }

        private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
        {
            GameObject gameObject = new GameObject("dotConnection", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().color = dotConnectionColor;

            return gameObject;
        }

        public class LineGraphVisualObject : IGraphVisualObject
        {
            public event EventHandler OnChangedGraphVisualObjectInfo;
            private GameObject dotGameObject;
            private GameObject dotConnectionGameObject;
            private LineGraphVisualObject lastVisualObject;
            private Vector2 graphPosition;

            public LineGraphVisualObject(GameObject dotGameObject, GameObject dotConnectionGameObject, LineGraphVisualObject lastVisualObject)
            {
                this.dotGameObject = dotGameObject;
                this.dotConnectionGameObject = dotConnectionGameObject;
                this.lastVisualObject = lastVisualObject;
                this.graphPosition = new Vector2();

                if (lastVisualObject != null)
                {
                    lastVisualObject.OnChangedGraphVisualObjectInfo += LastVisualObject_OnChangedGraphVisualObjectInfo;
                }
            }

            private void LastVisualObject_OnChangedGraphVisualObjectInfo(object sender, EventArgs e)
            {
                UpdateDotConnection();
            }

            public void SetGraphVisualObjectInfo(Vector2 graphPosition, float graphPositionWidth, string tooltipText)
            {
                RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = graphPosition;
                this.graphPosition = graphPosition;

                //if (!noConnect)
                //{
                    UpdateDotConnection();
                //}
                //noConnect = false;

                Button_UI dotButtonUI = dotGameObject.GetComponent<Button_UI>();
                dotButtonUI.MouseOverFunc = () =>
                {
                    ShowTooltip_Static(tooltipText, (graphPosition + new Vector2()));
                };
                dotButtonUI.MouseOutOnceFunc = () =>
                {
                    HideTooltip_Static();
                };


                if (OnChangedGraphVisualObjectInfo != null)
                {
                    OnChangedGraphVisualObjectInfo(this, EventArgs.Empty);
                }
            }

            public void Cleanup()
            {
                Destroy(dotGameObject);
                Destroy(dotConnectionGameObject);
            }

            public Vector2 GetGraphPosition()
            {
                RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();

                return rectTransform.anchoredPosition;
            }

            private void UpdateDotConnection()
            {
                if (dotConnectionGameObject != null)
                {
                    RectTransform dotConnectionRectTransform = dotConnectionGameObject.GetComponent<RectTransform>();
                    Vector2 dir = (lastVisualObject.GetGraphPosition() - graphPosition).normalized;
                    float distance = Vector2.Distance(graphPosition, lastVisualObject.GetGraphPosition());
                    dotConnectionRectTransform.sizeDelta = new Vector2(distance, 2f);
                    dotConnectionRectTransform.anchorMin = new Vector2(0, 0);
                    dotConnectionRectTransform.anchorMax = new Vector2(0, 0);
                    dotConnectionRectTransform.anchoredPosition = graphPosition + dir* distance * 0.5f;
                    dotConnectionRectTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(dir));
                }       
            }

            public GameObject GetDotGameObject()
            {
                return dotGameObject;
            }
        }
    }
}
