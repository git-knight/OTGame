using NotSoSimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ActiveQuestsView : MonoBehaviour
{
    GameObject questList;
    GameObject questSelection;
    Text questDescription;

    // Start is called before the first frame update
    void Start()
    {
        UICommon.FadeIn(gameObject);
    }


    public void Initialize(JSONArray quests)
    {
        questList = transform.Find("background/Scroll View/Viewport/Content").gameObject;
        questSelection = transform.Find("background/Scroll View/Viewport/Selection").gameObject;
        questDescription = transform.Find("background/QuestDescription/Text").GetComponent<Text>();

        for (int i = 1; i < questList.transform.childCount; i++)
            Destroy(questList.transform.GetChild(1).gameObject);

        int questIndex = 0;
        var listItemTemplate = questList.transform.Find("QuestNameTemplate");
        listItemTemplate.gameObject.SetActive(false);
        foreach (var quest in quests)
        {
            var questButton = Instantiate(listItemTemplate, questList.transform, false);
            questButton.gameObject.SetActive(true);
            questButton.name = "Quest [" + quest["title"].AsString + "]";
            questButton.GetComponent<Text>().text = quest["title"].AsString;

            //questButton.transform.localPosition += Vector3.down * 21 * questIndex;

            questButton.GetComponent<Button>().onClick.AddListener(() => { ShowQuest(questIndex, quest); });

            questIndex++;
        }

        ShowQuest(0, quests[0]);
    }

    public void ShowQuest(int ind, JSONNode quest)
    {
        var questButton = questList.transform.GetChild(ind + 1);

        var selLoc = questSelection.transform as RectTransform;
        selLoc.localPosition = new Vector3(selLoc.localPosition.x, questButton.localPosition.y, selLoc.localPosition.z);

        questDescription.text = quest["description"].AsString + "\n\n"
            + string.Join("\n", quest["completionInfo"].AsArray.Select(x => "<color=" + (x["isCompleted"].AsBool.Value ? "green" : "red") + ">" + x["statusText"].AsString + "</color>"));
    }
}
