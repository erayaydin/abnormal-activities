using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Horde.Abnormal.Arch;
using Horde.Abnormal.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Horde.Abnormal.Input
{
    public class InputHandler : Singleton<InputHandler>
    {
        #region Events

        /// <summary>
        /// Event will be called, when the input handler initialized.
        /// </summary>
        public static event Action OnInputsInitialized;

        /// <summary>
        /// Event will be called, when the input devices will be updated or changed.
        /// </summary>
        public static event Action<List<Device>> OnDevicesUpdated;
        
        /// <summary>
        /// Event will be called, when the input device status will be changed.
        /// </summary>
        public static event Action<Device, InputDeviceChange> OnDeviceStatusChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Is input handler initialized?
        /// </summary>
        /// <value>
        /// True if the input handler is initialized, otherwise false.
        /// </value>
        public static bool IsReady => Instance._isReady;
        
        /// <summary>
        /// Get current active device.
        /// </summary>
        public static Device CurrentDevice
        {
            get
            {
                if (Instance._activeDevice != Device.None)
                    return Instance._activeDevice;
                
                Instance.RefreshConnectedDevices();
                Instance.RefreshActiveDevice();
                
                return Instance._activeDevice;
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Input action asset to read maps and bindings.
        /// </summary>
        [SerializeField]
        [Tooltip("Input action asset to read maps and bindings.")]
        private InputActionAsset inputActionAsset;
        
        /// <summary>
        /// Default device to assign as active device.
        /// </summary>
        [SerializeField]
        [Tooltip("Default device to assign as active device.")]
        private Device defaultDevice = Device.None;

        /// <summary>
        /// Default input action map to use.
        /// </summary>
        [SerializeField]
        [Tooltip("Default input action map to use.")]
        private string defaultMap;

        /// <summary>
        /// List of action schemes, representing different sets of input actions and bindings.
        /// </summary>
        private readonly List<ActionScheme> _actionSchemes = new();

        /// <summary>
        /// The default input device stored in player preferences. This is used to remember the player's last used input device.
        /// </summary>
        private Device _playerPreferencesInputDevice;

        /// <summary>
        /// List of currently connected input devices.
        /// </summary>
        private readonly List<Device> _connectedDevices = new();

        /// <summary>
        /// Indicates whether the initialization of the input handler is completed.
        /// </summary>
        private bool _isReady;

        /// <summary>
        /// The current active input device. This device is considered as the primary source of input.
        /// </summary>
        private Device _activeDevice = Device.None;

        /// <summary>
        /// Path to the user input override file. This file is used to store custom input bindings set by the user.
        /// </summary>
        private readonly string _userInputOverridePath =
            Path.Combine(Application.dataPath, "Data", OverrideInputFileName);

        /// <summary>
        /// List used to remember the state of toggle buttons to ensure that a button press is only registered once.
        /// </summary>
        /// <see cref="ReadButtonOnce"/>
        private static readonly List<ButtonMemory> ButtonMemories = new();
        
        /// <summary>
        /// Unique key used to retrieve or store the default input device in player preferences.
        /// </summary>
        private const string PlayerPreferencesInputDeviceKey = "INPUT_DEVICE";

        /// <summary>
        /// Filename for the file where user input overrides are written.
        /// </summary>
        private const string OverrideInputFileName = "Inputs.xml";

        #endregion

        #region Unity Events

        /// <summary>
        /// Initializes the input handler asynchronously during the Awake phase of the MonoBehaviour lifecycle.
        /// This method sets up action schemes based on the input action asset, restores the player's preferred input device,
        /// refreshes the list of connected devices, applies any user-defined input overrides, and marks the input handler as ready.
        /// Finally, it enables the input action asset to start processing input.
        /// </summary>
        private async void Awake()
        {
            // Initialize action schemes from the input action asset.
            foreach (var actionMap in inputActionAsset.actionMaps)
            {
                var bindings = actionMap.actions.Select(binding => new ActionBinding(binding)).ToList();
                _actionSchemes.Add(new ActionScheme(actionMap.name, bindings));
            }
            
            // Restore the player's preferred input device from PlayerPrefs.
            _playerPreferencesInputDevice = (Device) PlayerPrefs.GetInt(PlayerPreferencesInputDeviceKey, 0);
            
            // Refresh the list of connected devices and the active device asynchronously.
            await Task.Run(() =>
            {
                RefreshConnectedDevices();
                RefreshActiveDevice();
            });
            
            // Notify subscribers that the list of connected devices has been updated.
            OnDevicesUpdated?.Invoke(_connectedDevices);
            
            // Apply any user-defined input overrides.
            ApplyOverrides();
            
            // Mark the input handler as ready for use.
            _isReady = true;
            
            // Enable the input action asset to start processing input.
            inputActionAsset.Enable();
            
            Debug.Log("[INPUT] Inputs initialized!");
        }

        /// <summary>
        /// Applies user-defined input overrides from an XML file.
        /// This method reads the XML file specified by <see cref="_userInputOverridePath"/>,
        /// parses each input binding override, and applies these overrides to the corresponding input actions.
        /// </summary>
        private void ApplyOverrides()
        {
            // Check if the override file exists. If not, exit the method.
            if (!File.Exists(_userInputOverridePath))
                return;

            // Load the XML document from the override file.
            var xmlDocument = XDocument.Load(new StreamReader(_userInputOverridePath));

            // If the XML document has no root element, exit the method.
            if (xmlDocument.Root == null)
                return;

            // Iterate through each node (input action) in the XML document.
            foreach (var node in xmlDocument.Root.Elements())
            {
                // Retrieve the action name and action map from the current node's attributes.
                var nodeName = node.Attribute("name")?.Value;
                var actionMap = node.Attribute("map")?.Value;

                // Iterate through each binding override within the current node.
                foreach (var binding in node.Elements())
                {
                    // Parse the binding index and the override control path from the current binding's attributes.
                    var index = int.Parse(binding.Attribute("index")?.Value ?? string.Empty);
                    var overrideControl = binding.Attribute("bind")?.Value ?? string.Empty;
                    // Format the override control path to the internal representation used by the input system.
                    var overridePath = FormatBindingPath(overrideControl);
                    // Attempt to retrieve a composite part for the current binding, if applicable.
                    var composite = CompositeOf(nodeName, index, actionMap);

                    // If the composite's pretty path matches the override control, skip this binding.
                    if (composite.GetPrettyPath() == overrideControl)
                        continue;
                    
                    // Retrieve the input action associated with the current binding.
                    var action = GetInputAction(nodeName, actionMap);
                    // Determine the path and display string for the override.
                    var path = overrideControl != "null" ? overridePath : overrideControl;
                    var display = path != "null" ? RealDisplayString(overridePath) : "None";

                    // Apply the binding override to the input action.
                    ApplyBindingOverride(new RebindContext
                    {
                        action = action,
                        actionMap = actionMap,
                        bindingIndex = index,
                        overridePath = path,
                        display = display
                    });
                }
            }
        }

        /// <summary>
        /// Applies a binding override to an input action. This method updates the action's binding with a new path and,
        /// if applicable, updates the composite binding's override path and display string.
        /// </summary>
        /// <param name="ctx">The context containing the rebind information, including the action to be overridden,
        /// the binding index, the new override path, and the display string for the override.</param>
        private static void ApplyBindingOverride(RebindContext ctx)
        {
            // Apply the binding override to the specified action at the given binding index with the new path.
            ctx.action.ApplyBindingOverride(ctx.bindingIndex, ctx.overridePath);

            // Attempt to retrieve a composite binding reference, if the binding is part of a composite.
            var composite = CompositeOfRef(ctx.action.name, ctx.bindingIndex, ctx.actionMap);
            if (composite == null)
                return;
            
            // If a composite is found, update its override path and display string with the new values.
            composite.overridePath = ctx.overridePath;
            composite.displayString = ctx.display;
        }

        /// <summary>
        /// Retrieves a reference to a composite part of an input binding.
        /// </summary>
        /// <param name="actionName">The name of the input action for which the composite part is being retrieved.</param>
        /// <param name="bindingIndex">The index of the binding within the action. This is used to identify the specific binding in cases where an action may have multiple bindings.</param>
        /// <param name="actionMap">The name of the action map that contains the action.</param>
        /// <returns>A <see cref="CompositePart"/> reference if found, representing the composite part of the specified binding. Returns null if no matching composite part is found.</returns>
        private static CompositePart CompositeOfRef(string actionName, int bindingIndex, string actionMap)
        {
            var bindings = Instance._actionSchemes
                .FirstOrDefault(x => x.Name == actionMap)
                ?.ActionBindings
                .FirstOrDefault(x => x.InputAction.name == actionName)
                ?.BindingLists;

            return (
                from binding in bindings
                from composite in binding.CompositeParts.Where(composite => composite.bindingIndex == bindingIndex)
                select composite
            ).FirstOrDefault();
        }

        /// <summary>
        /// Converts a binding path to a human-readable display string by formatting it properly.
        /// This method handles both simple and complex binding paths, ensuring they are presented
        /// in a readable format.
        /// </summary>
        /// <param name="bindingPath">The binding path to be converted into a display string.</param>
        /// <returns>A human-readable string that represents the input binding path.</returns>
        private static string RealDisplayString(string bindingPath)
        {
            if (!bindingPath.Contains("/"))
            {
                return bindingPath.Trim().ToTitleCase();
            }
            
            var split = bindingPath.Split('/')[1].Trim();

            var tmpDisplay = Regex.Replace(split, "([^A-Z ])([A-Z])", "$1 $2");
            return Regex.Replace(tmpDisplay, "([A-Z]+)([A-Z][^A-Z$])", "$1 $2")
                .Trim()
                .ToTitleCase();
        }

        /// <summary>
        /// Retrieves a clone of the composite part of an input binding.
        /// </summary>
        /// <param name="actionName">The name of the input action for which the composite part is being retrieved.</param>
        /// <param name="bindingIndex">The index of the binding within the action.</param>
        /// <param name="actionMap">The name of the action map that contains the action.</param>
        /// <returns>A clone of the <see cref="CompositePart">Composite Part</see> if found, representing the specific part of the composite binding. Returns null if no matching composite part is found.</returns>
        private static CompositePart CompositeOf(string actionName, int bindingIndex, string actionMap)
        {
            var bindings = Instance._actionSchemes
                .FirstOrDefault(x => x.Name == actionMap)
                ?.ActionBindings
                .FirstOrDefault(x => x.InputAction.name == actionName)
                ?.BindingLists;

            return (
                from binding in bindings
                from composite in binding.CompositeParts.Where(composite => composite.bindingIndex == bindingIndex)
                select composite.Clone()
            ).FirstOrDefault() as CompositePart;
        }

        /// <summary>
        /// Formats a given binding path into the internal representation used by the input system.
        /// This method is used to convert user-friendly binding paths into a format that the input system can understand.
        /// </summary>
        /// <param name="bindingPath">The binding path to format. This is typically a string that represents how an input is bound.</param>
        /// <returns>A string representing the internal format of the binding path, which is compatible with the input system.</returns>
        private static string FormatBindingPath(string bindingPath)
        {
            var split = bindingPath.Split('.');
            var newPath = $"<{split[0]}>/";

            for (var i = 1; i < split.Length; i++)
            {
                newPath += split[i];
            }

            return newPath;
        }

        /// <summary>
        /// Subscribes to the <see cref="InputSystem.onDeviceChange"/> event to handle input device changes.
        /// </summary>
        private void OnEnable() => InputSystem.onDeviceChange += OnDeviceChange;

        /// <summary>
        /// Coroutine that waits for the input handler to be fully initialized before invoking the <see cref="OnInputsInitialized"/> event.
        /// </summary>
        /// <returns>An enumerator needed for the coroutine to run.</returns>
        private IEnumerator Start()
        {
            yield return new WaitUntil(() => _isReady);
            OnInputsInitialized?.Invoke();
        }

        /// <summary>
        /// Unsubscribes from the <see cref="InputSystem.onDeviceChange"/> event to stop handling input device changes.
        /// </summary>
        private void OnDisable() => InputSystem.onDeviceChange -= OnDeviceChange;

        #endregion

        #region API

        /// <summary>
        /// Reads the value of the specified input action as a specific type.
        /// </summary>
        /// <param name="actionName">The name of the action to read the value from.</param>
        /// <param name="actionMap">Optional. The name of the action map that contains the action. If null, the default action map is used.</param>
        /// <typeparam name="T">The type to which the input value should be converted. Must be a struct.</typeparam>
        /// <returns>The value of the input action converted to the specified type. Returns the default value of the type if the action is not found or the value cannot be read.</returns>
        public static T ReadInput<T>(string actionName, string actionMap = null) where T : struct
        {
            var inputAction = GetInputAction(actionName, actionMap);
            return inputAction?.ReadValue<T>() ?? default;
        }

        /// <summary>
        /// Reads the value of a specified input action and returns it as a boolean.
        /// </summary>
        /// <param name="actionName">The name of the action to read the value from.</param>
        /// <param name="actionMap">Optional. The name of the action map that contains the action. If null, the default action map is used.</param>
        /// <returns>A boolean value indicating whether the button is pressed (true) or not (false). Returns false if the action is not found.</returns>
        /// <exception cref="NotSupportedException">Thrown when the input action is found but is not of type button.</exception>
        public static bool ReadButton(string actionName, string actionMap = null)
        {
            var inputAction = GetInputAction(actionName, actionMap);

            if (inputAction == null)
                return default;

            if (inputAction.type != InputActionType.Button)
                throw new NotSupportedException("Input action should be button!");

            return Convert.ToBoolean(inputAction.ReadValueAsObject());
        }

        /// <summary>
        /// Reads the value of a specified input action and ensures it is only processed once until released.
        /// </summary>
        /// <param name="instance">The MonoBehaviour instance calling this method. Used to track the button press state uniquely across different script instances.</param>
        /// <param name="actionName">The name of the action to read the value from.</param>
        /// <param name="actionMap">Optional. The name of the action map that contains the action. If null, the default action map is used.</param>
        /// <returns>True if the button is pressed for the first time since it was last released; otherwise, false.</returns>
        public static bool ReadButtonOnce(MonoBehaviour instance, string actionName, string actionMap = null)
        {
            if (!ReadButton(actionName, actionMap))
            {
                // If the button is not pressed, remove any existing memory of it being pressed.
                // This ensures that the button must be released before it can be considered pressed again.
                ButtonMemories.RemoveAll(x => x.Instance == instance && x.ActionName == actionName);
                return false;
            }
            
            // Check if the button press has already been recorded. If so, ignore subsequent presses until it's released.
            if (ButtonMemories.Any(x => x.Instance == instance && x.ActionName == actionName))
            {
                return false;
            }

            // Record the button press to prevent it from being processed again until released.
            ButtonMemories.Add(new ButtonMemory(instance, actionName));

            return true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles changes to input devices, such as when a device is added, removed, or its configuration changes.
        /// This method updates the list of connected devices and the active device based on the change.
        /// It also notifies subscribers about the device update and status change events.
        /// </summary>
        /// <param name="device">The input device that triggered the change event.</param>
        /// <param name="state">The type of change that occurred to the device.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the device state change is unknown.</exception>
        private void OnDeviceChange(InputDevice device, InputDeviceChange state)
        {
            var deviceType = GetDeviceType(device);
            
            switch (state)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                    AddDevice(deviceType);
                    break;
                case InputDeviceChange.Removed:
#pragma warning disable CS0618
                case InputDeviceChange.Destroyed:
#pragma warning restore CS0618
                case InputDeviceChange.Disconnected:
                case InputDeviceChange.Disabled:
                    RemoveDevice(deviceType);
                    break;
                case InputDeviceChange.Enabled:
                case InputDeviceChange.UsageChanged:
                case InputDeviceChange.ConfigurationChanged:
                case InputDeviceChange.SoftReset:
                case InputDeviceChange.HardReset:
                    // No action required for these states in the current implementation.
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            RefreshConnectedDevices();
            RefreshActiveDevice();
            OnDevicesUpdated?.Invoke(_connectedDevices);
            OnDeviceStatusChanged?.Invoke(deviceType, state);
        }
        
        /// <summary>
        /// Determines the type of the given input device.
        /// </summary>
        /// <param name="device">The input device to evaluate.</param>
        private static Device GetDeviceType(InputDevice device) => device switch
        {
            Keyboard => Device.MouseKeyboard,
            _ => Device.None
        };
        
        /// <summary>
        /// Add a new device to the list of connected devices if it is not already present.
        /// </summary>
        /// <param name="device">The input device to add to the list of connected devices.</param>
        private void AddDevice(Device device)
        {
            if (_connectedDevices.Contains(device))
                return;
            
            _connectedDevices.Add(device);
        }
        
        /// <summary>
        /// Remove the specified device from the list of connected devices.
        /// </summary>
        /// <param name="device">The input device to be removed from the list of connected devices.</param>
        private void RemoveDevice(Device device)
        {
            if (!_connectedDevices.Contains(device))
                return;
            
            _connectedDevices.Remove(device);
        }
        
        /// <summary>
        /// Refreshes the list of connected devices based on the current input device configuration.
        /// </summary>
        private void RefreshConnectedDevices()
        {
            if (Keyboard.current != null && Mouse.current != null)
                AddDevice(Device.MouseKeyboard);
            else
                RemoveDevice(Device.MouseKeyboard);
        }

        /// <summary>
        /// Determine active device from the connected devices list.
        /// </summary>
        private void RefreshActiveDevice()
        {
            if (_connectedDevices.Count <= 0)
                return;

            // If the player's preferred input device is connected, set it as the active device.
            if (_playerPreferencesInputDevice != Device.None && _connectedDevices.Contains(_playerPreferencesInputDevice))
            {
                _activeDevice = _playerPreferencesInputDevice;
                return;
            }

            // If a default device is specified and connected, set it as the active device.
            if (defaultDevice != Device.None && _connectedDevices.Contains(defaultDevice))
            {
                _activeDevice = defaultDevice;
                return;
            }

            // If no player preference or default device is available, set the first connected device as the active device.
            _activeDevice = _connectedDevices.First();
        }

        /// <summary>
        /// Get input action value from input action asset.
        /// </summary>
        /// <param name="actionName">The name of the action to retrieve the value from.</param>
        /// <param name="actionMap">Optional. The name of the action map that contains the action. If null, the default action map is used.</param>
        /// <returns>The value of the input action. Returns null if the action is not found.</returns>
        private static InputAction GetInputAction(string actionName, string actionMap)
        {
            actionMap ??= Instance.defaultMap;

            var inputActions = Instance.inputActionAsset;

            return string.IsNullOrEmpty(actionMap)
                ? inputActions.FindAction(actionName, true)
                : inputActions.FindActionMap(actionMap, true).FindAction(actionName, true);
        }

        #endregion
    }
}