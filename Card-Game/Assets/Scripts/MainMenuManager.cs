using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // ��� TextMeshPro

public class MainMenuManager : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject howToPlayPanel;
    public GameObject PlayPanel;
    public GameObject OfflinePanel;
    //public GameObject OnlinePanel;
    public GameObject LobbyPanel;
    public TMP_InputField playerCountInput; // ������� ��� TMP_InputField
    public TMP_Text errorMessageText; // ������� ��� TMP_Text ��� �� ������ ���������
    public bool PreviousSceneIsOffline = false;
    public bool PreviousSceneIsOnline = false;
    public GameObject MainMenuButtons;
    public GameObject OfflinePlayButton;
    public void PlayGame()
    {
        //SceneManager.LoadScene("PlayMenu");
    }
    public void PlayButton()
    {
        PlayPanel.SetActive(true);
        MainMenuPanel.SetActive(false);
    }

    public void ShowHowToPlay()
    {
        howToPlayPanel.SetActive(true);
        MainMenuButtons.SetActive(false);
    }

    public void HideHowToPlay()
    {
        howToPlayPanel.SetActive(false);
        if (PreviousSceneIsOffline == true)
        {
            OfflinePanel.SetActive(true);
            PreviousSceneIsOffline = false;
        }
        else if (PreviousSceneIsOnline == true)
        {
            LobbyPanel.SetActive(true);
            PreviousSceneIsOnline = false;
        }
        else
        {
            MainMenuButtons.SetActive(true);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    public void SelectOffline()
    {
        PlayPanel.SetActive(false); // ������� �� panel ��������
        MainMenuButtons.SetActive(false);
        OfflinePanel.SetActive(true); // ������� �� panel �������� �������
        OfflinePlayButton.SetActive(false);
    }
    public void SelectOnline()
    {
        PlayPanel.SetActive(false); // ������� �� panel ��������
        MainMenuPanel.SetActive(false);
        //OnlinePanel.SetActive(true); // ������� �� OnlinePanel
        LobbyPanel.SetActive(true);
    }


    // ������� ��� �� �������������� ��� ������� ��� �������
    public void ConfirmPlayerCount()
    {
        int playerCount;
        if (int.TryParse(playerCountInput.text, out playerCount)) // ����������� �� ������������ �� ������� �� ������� ������
        {
            if (playerCount >= 1 && playerCount <= 4) // �� � ������� ������� ����� ������ 1 ��� 4
            {
                errorMessageText.gameObject.SetActive(false); // �������� �� ������ ���������
                Debug.Log("������� �������: " + playerCount);
                OfflinePlayButton.SetActive(true);
                // ��� ������� �� ���������� �� ������ ��� �� ����������� �� ��� ������� ��� �������
            }
            else
            {
                errorMessageText.text = "� �������� ������� ������� ����� 4!!!"; // ����������� �� ������ ���������
                errorMessageText.gameObject.SetActive(true); // ������������� �� ������ ���������
            }
        }
        else
        {
            errorMessageText.text = "�������� �������� ���� ������ ������."; // ����������� ������ ��������� �� ��� ����� ������� �������
            errorMessageText.gameObject.SetActive(true); // ������������� �� ������ ���������
        }
       
    }
    private bool IsValidPlayerCount(string input)
    {
        if (int.TryParse(input, out int playerCount))
        {
            return playerCount >= 1 && playerCount <= 4; // ������� ������� �������
        }
        return false; // �� ������ ����
    }

    // ������� ��� �� Back (��������� ��� ����� �����)
    public void BackPlayPanel()
    {
        PlayPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }
    public void BackOfflinePanel()
    {
        OfflinePanel.SetActive(false);
        PlayPanel.SetActive(true);
    }
    public void BackOnlinePanel()
    {
        LobbyPanel.SetActive(false);
        PlayPanel.SetActive(true);
    }
    public void ShowHowToPlayInOffline()
    {
        howToPlayPanel.SetActive(true);
        PreviousSceneIsOffline = true;
        OfflinePanel.SetActive(false);
        MainMenuButtons.SetActive(false);
        errorMessageText.gameObject.SetActive(false);

        if (!IsValidPlayerCount(playerCountInput.text))
        {
            // �������� ��� ����� ��� Input Field
            playerCountInput.text = "";
            // �����������, �������� ��������� ���������
            errorMessageText.text = "Invalid input cleared. Please enter a valid number.";
        }
    }
    public void ShowHowToPlayInOnline()
    {
        howToPlayPanel.SetActive(true);
        PreviousSceneIsOnline = true;
        LobbyPanel.SetActive(false);
        MainMenuButtons.SetActive(false);
    }



}
