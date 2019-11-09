using NotSoSimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HeroStatsView : MonoBehaviour
{
    static string[] baseValueNames = { "level", "exp" };
    static string[] statNames = { "health", "attack", "defense", "minDamage", "maxDamage" };

    void Start()
    {
        UICommon.FadeIn(gameObject);
    }

    public void Initialize(JSONNode stats)
    {
        var statBasic = transform.Find("background/StatBasic").GetComponent<Text>();
        var statAddit = transform.Find("background/StatAdditionals").GetComponent<Text>();
        var statTotal = transform.Find("background/StatTotals").GetComponent<Text>();

        statBasic.text = string.Join("\n", baseValueNames.Select(x => stats[x].AsInt.Value).Concat(statNames.Select(x => stats["baseStats"][x].AsInt.Value)));
        statAddit.text = string.Join("\n", baseValueNames.Select(x => "").Concat(statNames.Select(x => stats["baseStats"][x] == stats["stats"][x] ? null : stats["stats"][x].AsInt - stats["baseStats"][x].AsInt).Select(x => (x.HasValue && x != 0) ? "+" + x : "")));
        statTotal.text = string.Join("\n", baseValueNames.Select(x => "").Concat(statNames.Select(x => "" + stats["stats"][x].AsInt)));

        statAddit.text = "<color=green>" + statAddit.text + "</color>";
    }
}
