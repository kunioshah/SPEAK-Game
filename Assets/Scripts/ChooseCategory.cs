using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChooseCategory : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_InputField inputField;
    [SerializeField] private Button button;



    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(SubmitForm);
    }

   

    // Update is called once per frame
    void Update()
    {
         if (Input.GetKeyUp(KeyCode.Return)) { SubmitForm(); }
    }

    private void SubmitForm()
    {
        GameVariables.CategroryName = inputField.text;
        SceneManager.LoadScene("Play Audio");
    }

}
