using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class plateInfo : MonoBehaviour {
	
	public string[] playerNames = new string[2];
	public string[] playerTeams = new string[2];
	public string weapon;
	
	public bool iscrit = false;
	
	
	UnityEngine.UI.Text player1Plate;
	UnityEngine.UI.Text player2Plate;
	UnityEngine.UI.Image weaponPlate;
	UnityEngine.UI.Image critPlate;

	public void Start () {
		player1Plate = transform.GetChild(0).transform.GetChild(0).GetComponent<UnityEngine.UI.Text>();
		player2Plate = transform.GetChild(0).transform.GetChild(3).GetComponent<UnityEngine.UI.Text>();
		weaponPlate = transform.GetChild(0).transform.GetChild(2).GetComponent<UnityEngine.UI.Image>();
		critPlate = transform.GetChild(0).transform.GetChild(1).GetComponent<UnityEngine.UI.Image>();
	}
	
	public void setWeapon ( string name, Sprite icon, int crit ) {
		weapon = name;
		
		var ocolor = weaponPlate.color;
		ocolor.a = 0f;
		
		weaponPlate.sprite = icon;
		
		critPlate.gameObject.SetActive(false);
		if (crit == 1) {
			critPlate.gameObject.SetActive(true);
			iscrit = true;
		}
		weaponPlate.transform.gameObject.SetActive(true);
		StartCoroutine(UpdatePlate(1));

	}
	
	public void setObjective ( Sprite icon, string text ) {
		weaponPlate.GetComponent<UnityEngine.UI.Image>().sprite = icon;
		weaponPlate.transform.gameObject.SetActive(true);
		critPlate.gameObject.SetActive(false);
		
		player2Plate.fontSize = 10;
		player2Plate.color =  new Color32( 0x3D , 0x39 , 0x23 , 0xFF );
		player2Plate.text = text;
	}
	
	public void setPlayer ( int id, string name, string team ) {
		UnityEngine.UI.Text[] plates = new UnityEngine.UI.Text[] {player1Plate, player2Plate};
		plates[id].text = name;
		
		Color team_color = Color.black;
		switch (team) {
		case "BLU":
			team_color = new Color32( 0x55 , 0x9A , 0xAD , 0xFF );
			break;
		case "RED":
			team_color = new Color32( 0xB6 , 0x57 , 0x4A , 0xFF );
			break;
		}
		plates[id].color = team_color;
		
		playerNames[id] = name;
		playerTeams[id] = team;
	}
	
	public IEnumerator UpdatePlate(int frameCount)
    {
        while (frameCount > 0)
        {
            frameCount--;
            yield return null;
        }
		critPlate.GetComponent<RectTransform>().anchoredPosition3D = weaponPlate.GetComponent<RectTransform>().anchoredPosition3D;
    }
	
}
