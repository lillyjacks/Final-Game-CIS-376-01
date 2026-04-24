using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class CreateBasicPlayerAnimator
{
    private const string BaseOutputFolder = "Assets/Animations";
    private const string GeneratedFolder = "Assets/Animations/Generated";

    [MenuItem("Tools/Player Animation/Create Basic Male Animator")]
    public static void CreateMaleAnimator()
    {
        CreateAnimatorController(
            "MalePlayerBasic",
            "HumanM@Idle01.fbx",
            "HumanM@Walk01_Forward.fbx",
            "HumanM@Run01_Forward.fbx",
            "HumanM@Jump01 - Begin.fbx",
            "HumanM@Fall01.fbx",
            "HumanM@Jump01 - Land.fbx");
    }

    [MenuItem("Tools/Player Animation/Create Basic Female Animator")]
    public static void CreateFemaleAnimator()
    {
        CreateAnimatorController(
            "FemalePlayerBasic",
            "HumanF@Idle01.fbx",
            "HumanF@Walk01_Forward.fbx",
            "HumanF@Run01_Forward.fbx",
            "HumanF@Jump01 - Begin.fbx",
            "HumanF@Fall01.fbx",
            "HumanF@Jump01 - Land.fbx");
    }

    private static void CreateAnimatorController(
        string controllerName,
        string idleClipName,
        string walkClipName,
        string runClipName,
        string jumpClipName,
        string fallClipName,
        string landClipName)
    {
        EnsureFolder("Assets", "Animations", BaseOutputFolder);
        EnsureFolder(BaseOutputFolder, "Generated", GeneratedFolder);

        AnimationClip idleClip = LoadClipByName(idleClipName);
        AnimationClip walkClip = LoadClipByName(walkClipName);
        AnimationClip runClip = LoadClipByName(runClipName);
        AnimationClip jumpClip = LoadClipByName(jumpClipName);
        AnimationClip fallClip = LoadClipByName(fallClipName);
        AnimationClip landClip = LoadClipByName(landClipName);

        if (idleClip == null || walkClip == null || runClip == null || jumpClip == null || fallClip == null || landClip == null)
        {
            Debug.LogError("Could not find one or more Kevin Iglesias clips. Make sure the Human Animations assets are imported.");
            return;
        }

        string controllerPath = $"{GeneratedFolder}/{controllerName}.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
        {
            AssetDatabase.DeleteAsset(controllerPath);
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.parameters = new AnimatorControllerParameter[0];

        AddParameters(controller);

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        stateMachine.anyStatePosition = new Vector3(-250f, 200f, 0f);
        stateMachine.entryPosition = new Vector3(-450f, 0f, 0f);
        stateMachine.exitPosition = new Vector3(550f, 0f, 0f);

        AnimatorState idleState = stateMachine.AddState("Idle", new Vector3(-50f, 0f, 0f));
        AnimatorState walkState = stateMachine.AddState("Walk", new Vector3(225f, -100f, 0f));
        AnimatorState runState = stateMachine.AddState("Run", new Vector3(225f, 100f, 0f));
        AnimatorState jumpState = stateMachine.AddState("Jump", new Vector3(500f, -175f, 0f));
        AnimatorState fallState = stateMachine.AddState("Fall", new Vector3(500f, 175f, 0f));
        AnimatorState landState = stateMachine.AddState("Land", new Vector3(750f, 0f, 0f));
        AnimatorState crouchState = stateMachine.AddState("Crouch", new Vector3(225f, 275f, 0f));

        idleState.motion = idleClip;
        walkState.motion = walkClip;
        runState.motion = runClip;
        jumpState.motion = jumpClip;
        fallState.motion = fallClip;
        landState.motion = landClip;
        crouchState.motion = idleClip;

        stateMachine.defaultState = idleState;

        AddTransition(idleState, walkState, 0f, false, AddConditionMode.If, "IsMoving");
        AddTransition(walkState, idleState, 0f, false, AddConditionMode.IfNot, "IsMoving");
        AddTransition(walkState, runState, 0f, false, AddConditionMode.If, "IsRunning");
        AddTransition(runState, walkState, 0f, false, AddConditionMode.IfNot, "IsRunning");
        AddTransition(runState, idleState, 0f, false, AddConditionMode.IfNot, "IsMoving");

        AddTransition(idleState, crouchState, 0f, false, AddConditionMode.If, "IsCrouching");
        AddTransition(walkState, crouchState, 0f, false, AddConditionMode.If, "IsCrouching");
        AddTransition(runState, crouchState, 0f, false, AddConditionMode.If, "IsCrouching");
        AddTransition(crouchState, idleState, 0f, false, AddConditionMode.IfNot, "IsCrouching");

        AddTransition(jumpState, fallState, 0.05f, false, AddConditionMode.If, "IsFalling");
        AddTransition(fallState, landState, 0.02f, false, AddConditionMode.If, "Grounded");
        AddExitTimeTransition(landState, idleState, 0.05f, 0.75f, AddConditionMode.IfNot, "IsMoving");
        AddExitTimeTransition(landState, walkState, 0.05f, 0.75f, AddConditionMode.If, "IsMoving");

        AnimatorStateTransition anyToJump = stateMachine.AddAnyStateTransition(jumpState);
        anyToJump.hasExitTime = false;
        anyToJump.duration = 0.05f;
        anyToJump.AddCondition(AnimatorConditionMode.If, 0f, "Jump");

        AnimatorStateTransition anyToFall = stateMachine.AddAnyStateTransition(fallState);
        anyToFall.hasExitTime = false;
        anyToFall.duration = 0.05f;
        anyToFall.AddCondition(AnimatorConditionMode.If, 0f, "IsFalling");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(controller);
        Debug.Log($"Created animator controller at {controllerPath}");
    }

    private static void AddParameters(AnimatorController controller)
    {
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
        controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
        controller.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsCrouching", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsFalling", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
    }

    private static void AddTransition(
        AnimatorState fromState,
        AnimatorState toState,
        float duration,
        bool hasExitTime,
        AddConditionMode firstMode,
        string firstParameter)
    {
        AnimatorStateTransition transition = fromState.AddTransition(toState);
        transition.hasExitTime = hasExitTime;
        transition.duration = duration;
        transition.AddCondition(ConvertCondition(firstMode), 0f, firstParameter);
    }

    private static void AddExitTimeTransition(
        AnimatorState fromState,
        AnimatorState toState,
        float duration,
        float exitTime,
        AddConditionMode firstMode,
        string firstParameter)
    {
        AnimatorStateTransition transition = fromState.AddTransition(toState);
        transition.hasExitTime = true;
        transition.exitTime = exitTime;
        transition.duration = duration;
        transition.AddCondition(ConvertCondition(firstMode), 0f, firstParameter);
    }

    private static void AddTransition(
        AnimatorState fromState,
        AnimatorState toState,
        float duration,
        bool hasExitTime,
        AddConditionMode firstMode,
        string firstParameter,
        AddConditionMode secondMode,
        string secondParameter)
    {
        AnimatorStateTransition transition = fromState.AddTransition(toState);
        transition.hasExitTime = hasExitTime;
        transition.duration = duration;
        transition.AddCondition(ConvertCondition(firstMode), 0f, firstParameter);
        transition.AddCondition(ConvertCondition(secondMode), 0f, secondParameter);
    }

    private static AnimatorConditionMode ConvertCondition(AddConditionMode mode)
    {
        return mode == AddConditionMode.If ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot;
    }

    private static AnimationClip LoadClipByName(string clipFileName)
    {
        string clipNameWithoutExtension = Path.GetFileNameWithoutExtension(clipFileName);
        string[] guids = AssetDatabase.FindAssets($"{clipNameWithoutExtension} t:AnimationClip");
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.EndsWith(clipFileName))
            {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                AnimationClip clip = assets.OfType<AnimationClip>().FirstOrDefault(item => item.name != "__preview__Take 001");
                if (clip != null)
                {
                    return clip;
                }
            }
        }

        return null;
    }

    private static void EnsureFolder(string parentFolder, string childFolderName, string expectedPath)
    {
        if (AssetDatabase.IsValidFolder(expectedPath))
        {
            return;
        }

        AssetDatabase.CreateFolder(parentFolder, childFolderName);
    }

    private enum AddConditionMode
    {
        If,
        IfNot
    }
}
