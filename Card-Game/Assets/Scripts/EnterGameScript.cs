using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnterGameScript : NetworkBehaviour
{
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Update()
	{
		Button myButton = GetComponent<Button>();
		Image myImage = GetComponent<Image>();
		if (IsHost)
		{
			myButton.enabled = true;
			myImage.enabled = true;
			transform.GetChild(0).gameObject.SetActive(true);
			myButton.onClick.AddListener(() =>
			{
				loadNextScene();
			});
			return;
		}
		myButton.onClick.RemoveAllListeners();
		transform.GetChild(0).gameObject.SetActive(true);
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
