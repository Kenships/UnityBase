namespace _Project.Scripts.Core.SceneLoading.Interfaces
{
    public interface ISceneBuilder
    {
        SceneController.SceneLoadingStrategy NewStrategy();
    }
}
