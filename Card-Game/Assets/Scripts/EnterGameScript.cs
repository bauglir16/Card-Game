using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnterGameScript : NetworkBehaviour
{
	Button myButton;
	Image myImage;

	private void Start()
	{
		myButton = GetComponent<Button>();
		myImage = GetComponent<Image>();
	}
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Update()
	{
		
		if (IsHost)
		{
			myButton.enabled = true;
			myImage.enabled = true;
			transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().enabled = true;
			myButton.onClick.AddListener(() =>
			{
				loadNextScene();
			});
			return;
		}
		myButton.onClick.RemoveAllListeners();
		transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().enabled = false;
		myButton.enabled = false;
		myImage.enabled = false;
	}

	void loadNextScene()
	{
		//loadNextSceneClientRpc();
		NetworkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
	}

	[ClientRpc]
	void loadNextSceneClientRpc()
	{
		SceneManager.LoadScene(1, LoadSceneMode.Single);
	}

	
}
