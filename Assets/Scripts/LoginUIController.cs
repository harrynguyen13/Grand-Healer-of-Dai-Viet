using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginUIController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject loginRoot;
    [SerializeField] private GameObject registerRoot;

    [Header("Login Input")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("Login Error Text")]
    [SerializeField] private TMP_Text usernameErrorText;
    [SerializeField] private TMP_Text passwordErrorText;

    [Header("Register Input")]
    [SerializeField] private TMP_InputField registerUsernameInput;
    [SerializeField] private TMP_InputField registerPasswordInput;
    [SerializeField] private TMP_InputField registerConfirmPasswordInput;

    [Header("Register Error Text")]
    [SerializeField] private TMP_Text registerUsernameErrorText;
    [SerializeField] private TMP_Text registerPasswordErrorText;
    [SerializeField] private TMP_Text registerConfirmPasswordErrorText;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "SampleScene";

    private void Awake()
    {
        SetupAllInputFields();

        ClearLoginInputs();
        ClearRegisterInputs();
        ClearAllErrors();

        ShowLoginPanel();
    }

    private void OnEnable()
    {
        if (usernameInput != null)
            usernameInput.onValueChanged.AddListener(OnLoginUsernameChanged);

        if (passwordInput != null)
            passwordInput.onValueChanged.AddListener(OnLoginPasswordChanged);

        if (registerUsernameInput != null)
            registerUsernameInput.onValueChanged.AddListener(OnRegisterUsernameChanged);

        if (registerPasswordInput != null)
            registerPasswordInput.onValueChanged.AddListener(OnRegisterPasswordChanged);

        if (registerConfirmPasswordInput != null)
            registerConfirmPasswordInput.onValueChanged.AddListener(OnRegisterConfirmPasswordChanged);
    }

    private void OnDisable()
    {
        if (usernameInput != null)
            usernameInput.onValueChanged.RemoveListener(OnLoginUsernameChanged);

        if (passwordInput != null)
            passwordInput.onValueChanged.RemoveListener(OnLoginPasswordChanged);

        if (registerUsernameInput != null)
            registerUsernameInput.onValueChanged.RemoveListener(OnRegisterUsernameChanged);

        if (registerPasswordInput != null)
            registerPasswordInput.onValueChanged.RemoveListener(OnRegisterPasswordChanged);

        if (registerConfirmPasswordInput != null)
            registerConfirmPasswordInput.onValueChanged.RemoveListener(OnRegisterConfirmPasswordChanged);
    }

    private void SetupAllInputFields()
    {
        SetupTextInput(usernameInput);
        SetupPasswordInput(passwordInput);

        SetupTextInput(registerUsernameInput);
        SetupPasswordInput(registerPasswordInput);
        SetupPasswordInput(registerConfirmPasswordInput);
    }

    private void SetupTextInput(TMP_InputField input)
    {
        if (input == null) return;

        input.contentType = TMP_InputField.ContentType.Standard;
        input.inputType = TMP_InputField.InputType.Standard;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.characterLimit = 20;
        input.richText = false;
    }

    private void SetupPasswordInput(TMP_InputField input)
    {
        if (input == null) return;

        input.contentType = TMP_InputField.ContentType.Custom;
        input.inputType = TMP_InputField.InputType.Password;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.characterValidation = TMP_InputField.CharacterValidation.None;
        input.characterLimit = 20;
        input.asteriskChar = '*';
        input.richText = false;
    }

    public void OnLoginClicked()
    {
        ClearLoginErrors();

        if (AuthManager.Instance == null)
        {
            ShowUsernameError("Lỗi hệ thống: chưa có AuthManager.");
            return;
        }

        string username = usernameInput != null ? usernameInput.text.Trim() : "";
        string password = passwordInput != null ? passwordInput.text.Trim() : "";

        bool hasError = false;

        if (string.IsNullOrEmpty(username))
        {
            ShowUsernameError("Vui lòng nhập tên người chơi.");
            hasError = true;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowPasswordError("Vui lòng nhập mật khẩu.");
            hasError = true;
        }

        if (hasError) return;

        bool loginSuccess = AuthManager.Instance.Login(username, password, out string message);

        if (!loginSuccess)
        {
            ShowPasswordError(message);
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void OnRegisterClicked()
    {
        ShowRegisterPanel();
    }

    public void OnRegisterSubmitClicked()
    {
        ClearRegisterErrors();

        if (AuthManager.Instance == null)
        {
            ShowRegisterUsernameError("Lỗi hệ thống: chưa có AuthManager.");
            return;
        }

        string username = registerUsernameInput != null ? registerUsernameInput.text.Trim() : "";
        string password = registerPasswordInput != null ? registerPasswordInput.text.Trim() : "";
        string confirmPassword = registerConfirmPasswordInput != null ? registerConfirmPasswordInput.text.Trim() : "";

        bool hasError = false;

        if (string.IsNullOrEmpty(username))
        {
            ShowRegisterUsernameError("Vui lòng nhập tên người chơi.");
            hasError = true;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowRegisterPasswordError("Vui lòng nhập mật khẩu.");
            hasError = true;
        }

        if (string.IsNullOrEmpty(confirmPassword))
        {
            ShowRegisterConfirmPasswordError("Vui lòng nhập lại mật khẩu.");
            hasError = true;
        }

        if (hasError) return;

        if (password != confirmPassword)
        {
            ShowRegisterConfirmPasswordError("Mật khẩu nhập lại không khớp.");

            if (registerConfirmPasswordInput != null)
                registerConfirmPasswordInput.ActivateInputField();

            return;
        }

        bool registerSuccess = AuthManager.Instance.Register(username, password, out string message);

        if (!registerSuccess)
        {
            ShowRegisterUsernameError(message);
            return;
        }

        PlayerPrefs.SetString("PlayerName", username);
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameSceneName);
    }

    public void OnBackToLoginClicked()
    {
        ShowLoginPanel();
    }

    private void ShowLoginPanel()
    {
        if (loginRoot != null)
            loginRoot.SetActive(true);

        if (registerRoot != null)
            registerRoot.SetActive(false);

        ClearRegisterErrors();
    }

    private void ShowRegisterPanel()
    {
        ClearLoginErrors();
        ClearRegisterInputs();
        ClearRegisterErrors();

        if (loginRoot != null)
            loginRoot.SetActive(false);

        if (registerRoot != null)
            registerRoot.SetActive(true);
    }

    private void ClearLoginInputs()
    {
        ClearInput(usernameInput);
        ClearInput(passwordInput);
    }

    private void ClearRegisterInputs()
    {
        ClearInput(registerUsernameInput);
        ClearInput(registerPasswordInput);
        ClearInput(registerConfirmPasswordInput);
    }

    private void ClearInput(TMP_InputField input)
    {
        if (input == null) return;

        input.DeactivateInputField();
        input.SetTextWithoutNotify("");
        input.caretPosition = 0;
        input.selectionAnchorPosition = 0;
        input.selectionFocusPosition = 0;
        input.ForceLabelUpdate();
    }

    private void OnLoginUsernameChanged(string value)
    {
        ClearUsernameError();

        // Nếu đang báo "sai tài khoản/mật khẩu", sửa username thì xóa luôn lỗi password.
        ClearPasswordError();
    }

    private void OnLoginPasswordChanged(string value)
    {
        ClearPasswordError();

        if (passwordInput != null)
            passwordInput.ForceLabelUpdate();
    }

    private void OnRegisterUsernameChanged(string value)
    {
        ClearRegisterUsernameError();
    }

    private void OnRegisterPasswordChanged(string value)
    {
        ClearRegisterPasswordError();

        // Nếu trước đó lỗi "mật khẩu nhập lại không khớp",
        // sửa mật khẩu chính thì lỗi nhập lại cũng nên biến mất.
        ClearRegisterConfirmPasswordError();

        if (registerPasswordInput != null)
            registerPasswordInput.ForceLabelUpdate();
    }

    private void OnRegisterConfirmPasswordChanged(string value)
    {
        ClearRegisterConfirmPasswordError();

        if (registerConfirmPasswordInput != null)
            registerConfirmPasswordInput.ForceLabelUpdate();
    }

    private void ShowUsernameError(string message)
    {
        if (usernameErrorText != null)
            usernameErrorText.text = message;
    }

    private void ShowPasswordError(string message)
    {
        if (passwordErrorText != null)
            passwordErrorText.text = message;
    }

    private void ShowRegisterUsernameError(string message)
    {
        if (registerUsernameErrorText != null)
            registerUsernameErrorText.text = message;
    }

    private void ShowRegisterPasswordError(string message)
    {
        if (registerPasswordErrorText != null)
            registerPasswordErrorText.text = message;
    }

    private void ShowRegisterConfirmPasswordError(string message)
    {
        if (registerConfirmPasswordErrorText != null)
            registerConfirmPasswordErrorText.text = message;
    }

    private void ClearUsernameError()
    {
        if (usernameErrorText != null)
            usernameErrorText.text = "";
    }

    private void ClearPasswordError()
    {
        if (passwordErrorText != null)
            passwordErrorText.text = "";
    }

    private void ClearLoginErrors()
    {
        ClearUsernameError();
        ClearPasswordError();
    }

    private void ClearRegisterUsernameError()
    {
        if (registerUsernameErrorText != null)
            registerUsernameErrorText.text = "";
    }

    private void ClearRegisterPasswordError()
    {
        if (registerPasswordErrorText != null)
            registerPasswordErrorText.text = "";
    }

    private void ClearRegisterConfirmPasswordError()
    {
        if (registerConfirmPasswordErrorText != null)
            registerConfirmPasswordErrorText.text = "";
    }

    private void ClearRegisterErrors()
    {
        ClearRegisterUsernameError();
        ClearRegisterPasswordError();
        ClearRegisterConfirmPasswordError();
    }

    private void ClearAllErrors()
    {
        ClearLoginErrors();
        ClearRegisterErrors();
    }
}