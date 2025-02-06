using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnterGameScript : NetworkBehaviour
{
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	[SerializeField] GameObject enterButton;
	void Update()
	{
		if (IsHost && NetworkManager.Singleton.ConnectedClientsList.Count > 1)
		{
			enterButton.SetActive(true);
			return;
		}
		enterButton.SetActive(false);
	}

	public void loadNextScene()
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
