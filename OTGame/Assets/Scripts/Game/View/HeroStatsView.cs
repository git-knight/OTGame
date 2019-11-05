using NotSoSimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HeroStatsView : MonoBehaviour
{
    static string[] statBasicNames = { "level", "health", "attack", "defense", "exp", "basicMinDamage", "basicMaxDamage" };
    static string[] statTotalNames = { "level", "health", "actualAttack", "actualDefense", "exp", "minDamage", "maxDamage" };

    void Start()
    {
        UICommon.FadeIn(gameObject);
    }

    public void Initialize(JSONNode stats)
    {
        var statBasic = transform.Find("background/StatBasic").GetComponent<Text>();
        var statAddit = transform.Find("background/StatAdditionals").GetComponent<Text>();
        var statTotal = transform.Find("background/StatTotals").GetComponent<Text>();

        statBasic.text = string.Join("\n", statBasicNames.Select(x => stats[x].AsInt.Value));
        statAddit.text = string.Join("\n", statBasicNames.Zip(statTotalNames, (a, b) => a == b ? null : stats[b].AsInt - stats[a].AsInt).Select(x => (x.HasValue && x != 0) ? "+" + x : ""));
        statTotal.text = string.Join("\n", statTotalNames.Select(x => stats[x].AsInt.Value));

        statAddit.text = "<color=green>" + statAddit.text + "</color>";
    }
}
