using System.Linq;
using Beam;
using Beam.Models;
using BeamPlayerClient.Model;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks; // For async/await using UniTask

public class BeamUI : MonoBehaviour
{
    // set your Publishable(!) API key
    [SerializeField] private string BEAM_API_KEY;

    private BeamClient beamClient;

    private VisualElement rootElement;

    // References to UI elements
    private Button connectToGameButton;
    private Button createSessionButton;
    private Button revokeSessionButton;
    private Button signOperationButton;
    private Button checkHealthButton;

    private TextField entityIdInput;
    private TextField operationIdInput;
    private TextField responsesInput;

    private void OnEnable()
    {
        beamClient = gameObject.AddComponent<BeamClient>()
            .SetBeamApiKey(BEAM_API_KEY)
            .SetEnvironment(BeamEnvironment.Testnet)
            .SetDebugLogging(true);

        // Clone and attach the UXML template
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument component not found on the GameObject.");
            return;
        }

        rootElement = uiDocument.rootVisualElement;

        // Initialize UI elements
        connectToGameButton = rootElement.Q<Button>("ConnectToGameButton");
        createSessionButton = rootElement.Q<Button>("CreateSessionButton");
        revokeSessionButton = rootElement.Q<Button>("RevokeSessionButton");
        signOperationButton = rootElement.Q<Button>("SignOperationButton");
        checkHealthButton = rootElement.Q<Button>("CheckHealthButton");

        entityIdInput = rootElement.Q<TextField>("EntityIdInput");
        operationIdInput = rootElement.Q<TextField>("OperationIdInput");
        responsesInput = rootElement.Q<TextField>("ResponsesInput");

        responsesInput.multiline = true;
        responsesInput.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
        responsesInput.style.whiteSpace = WhiteSpace.Normal;

        // Attach listeners
        connectToGameButton.clicked += async () => await OnConnectToGameClicked();
        createSessionButton.clicked += async () => await OnCreateSessionClicked();
        revokeSessionButton.clicked += async () => await OnRevokeSessionClicked();
        signOperationButton.clicked += async () => await OnSignOperationClicked();
        checkHealthButton.clicked += async () => await OnCheckHealthClicked();
    }

    private void OnDisable()
    {
        // Detach listeners
        connectToGameButton.clicked -= async () => await OnConnectToGameClicked();
        createSessionButton.clicked -= async () => await OnCreateSessionClicked();
        revokeSessionButton.clicked -= async () => await OnRevokeSessionClicked();
        signOperationButton.clicked -= async () => await OnSignOperationClicked();
        checkHealthButton.clicked -= async () => await OnCheckHealthClicked();
    }

    // Async actions for button clicks
    private async UniTask OnConnectToGameClicked()
    {
        AppendToResponseInput("Connect to game button clicked.", true);
        var entityId = GetEntityIdInputValue();

        var result = await beamClient.ConnectUserToGameAsync(entityId);
        if (result.Status == BeamResultType.Success)
        {
            AppendToResponseInput(
                $"User connected to the game with entityId: {entityId}.");

            var user = await beamClient.UsersApi.GetUserAsync(entityId);
            AppendToResponseInput($"User's wallet address: {user.Wallets.First(w => w.ChainId == 13337)?.Address}");
        }
    }
    
    private async UniTask OnCreateSessionClicked()
    {
        AppendToResponseInput("Create Session button clicked.", true);
        var entityId = GetEntityIdInputValue();

        var existingSession = await beamClient.GetActiveSessionAsync(entityId);
        if (existingSession.Status == BeamResultType.Success)
        {
            AppendToResponseInput(
                $"Active session already exists: {existingSession.Result.SessionAddress} until {existingSession.Result.EndTime.ToString()}.");
            return;
        }

        var newSession = await beamClient.CreateSessionAsync(entityId);
        if (newSession.Status == BeamResultType.Success)
        {
            AppendToResponseInput(
                $"New session created: {newSession.Result.SessionAddress} until {newSession.Result.EndTime.ToString()}.");
        }
        else
        {
            AppendToResponseInput($"Failed to create a new session: {newSession.Error}.");
        }
    }

    private async UniTask OnRevokeSessionClicked()
    {
        AppendToResponseInput("Revoke Session button clicked.", true);
        var entityId = GetEntityIdInputValue();
        var existingSession = await beamClient.GetActiveSessionAsync(entityId);
        if (existingSession.Status != BeamResultType.Success)
        {
            AppendToResponseInput("No active session.");
            return;
        }

        var revokeResult = await beamClient.RevokeSessionAsync(entityId, existingSession.Result.SessionAddress);
        if (revokeResult.Status == BeamResultType.Success)
        {
            AppendToResponseInput("Session revoked.");
        }
        else
        {
            AppendToResponseInput($"Failed to revoke session: {revokeResult.Error}.");
        }
    }

    private async UniTask OnSignOperationClicked()
    {
        AppendToResponseInput("Sign Operation button clicked.", true);
        var operationId = GetOperationIdInputValue();
        if (string.IsNullOrWhiteSpace(operationId))
        {
            AppendToResponseInput("Input operation Id you want to sign");
            return;
        }

        var entityId = GetEntityIdInputValue();
        var signingResult = await beamClient.SignOperationAsync(entityId, operationId);
        if (signingResult.Status == BeamResultType.Success)
        {
            AppendToResponseInput($"Operation signed: {signingResult.Result}.");
        }
        else
        {
            AppendToResponseInput($"Failed to sign operation: {signingResult.Error}.");
        }
    }

    private async UniTask OnCheckHealthClicked()
    {
        AppendToResponseInput("Check Health button clicked.", true);

        var healthResult = await beamClient.HealthApi.CheckAsync();

        AppendToResponseInput(healthResult.ToJson());
    }

    // Methods to get current input values
    public string GetEntityIdInputValue()
    {
        return entityIdInput.value;
    }

    public string GetOperationIdInputValue()
    {
        return operationIdInput.value;
    }

    // Method to append text to the responses input
    public void AppendToResponseInput(string text, bool reset = false)
    {
        if (reset)
        {
            responsesInput.value = string.Empty;
        }

        responsesInput.value += text + "\n";
        responsesInput.MarkDirtyRepaint(); // Refresh UI to reflect changes
    }
}