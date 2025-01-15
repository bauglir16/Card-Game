using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // Για TextMeshPro

public class MainMenuManager : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject howToPlayPanel;
    public GameObject PlayPanel;
    public GameObject OfflinePanel;
    //public GameObject OnlinePanel;
    public GameObject LobbyPanel;
    public TMP_InputField playerCountInput; // Αναφορά στο TMP_InputField
    public TMP_Text errorMessageText; // Αναφορά στο TMP_Text για το μήνυμα σφάλματος
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
        PlayPanel.SetActive(false); // Κλείνει το panel επιλογών
        MainMenuButtons.SetActive(false);
        OfflinePanel.SetActive(true); // Ανοίγει το panel επιλογής παικτών
        OfflinePlayButton.SetActive(false);
    }
    public void SelectOnline()
    {
        PlayPanel.SetActive(false); // Κλείνει το panel επιλογών
        MainMenuPanel.SetActive(false);
        //OnlinePanel.SetActive(true); // Ανοίγει το OnlinePanel
        LobbyPanel.SetActive(true);
    }


    // Μέθοδος για να επιβεβαιώσουμε την επιλογή των παικτών
    public void ConfirmPlayerCount()
    {
        int playerCount;
        if (int.TryParse(playerCountInput.text, out playerCount)) // Προσπαθούμε να μετατρέψουμε το κείμενο σε ακέραιο αριθμό
        {
            if (playerCount >= 1 && playerCount <= 4) // Αν ο αριθμός παικτών είναι μεταξύ 1 και 4
            {
                errorMessageText.gameObject.SetActive(false); // Κρύβουμε το μήνυμα σφάλματος
                Debug.Log("Αριθμός παικτών: " + playerCount);
                OfflinePlayButton.SetActive(true);
                // Εδώ μπορείς να προσθέσεις τη λογική για να προχωρήσεις με την επιλογή των παικτών
            }
            else
            {
                errorMessageText.text = "Ο μέγιστος αριθμός παικτών είναι 4!!!"; // Εμφανίζουμε το μήνυμα σφάλματος
                errorMessageText.gameObject.SetActive(true); // Ενεργοποιούμε το μήνυμα σφάλματος
            }
        }
        else
        {
            errorMessageText.text = "Παρακαλώ εισάγετε έναν έγκυρο αριθμό."; // Εμφανίζουμε μήνυμα σφάλματος αν δεν είναι έγκυρος αριθμός
            errorMessageText.gameObject.SetActive(true); // Ενεργοποιούμε το μήνυμα σφάλματος
        }
       
    }
    private bool IsValidPlayerCount(string input)
    {
        if (int.TryParse(input, out int playerCount))
        {
            return playerCount >= 1 && playerCount <= 4; // Έγκυρος αριθμός παικτών
        }
        return false; // Μη έγκυρη τιμή
    }

    // Μέθοδος για το Back (επιστροφή στο κύριο μενού)
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
            // Διαγραφή της τιμής στο Input Field
            playerCountInput.text = "";
            // Προαιρετικά, εμφάνιση μηνύματος σφάλματος
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
