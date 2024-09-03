namespace WebTree.Logic;

public enum OperationResult
{
    Success = 0,
    ErrNoTree = 101,
    ErrNoParrentNodeInTree = 201,
    ErrParrentHasNodeWithThisName = 202,
    ErrNoNodeInTree = 203,
    ErrNothingToRename = 204,
    ErrNodeHaveChildren = 205,
}