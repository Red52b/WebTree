using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using WebTree.Logic;

namespace WebTree.Api;

/// <summary>
/// Контроллер работы с деревом. 
/// </summary>
[ApiController]
public class UserTreeController : ControllerBase
{
    private readonly RequestEventForLog _logMessage;
    private readonly Forest _trees;

    /// <summary>
    /// Контроллер работы с деревом. 
    /// </summary>
    public UserTreeController(RequestEventForLog logMessage, Forest trees)
    {
        _logMessage = logMessage;
        _trees = trees;
    }

    /// <summary>
    /// Returns your entire tree. If your tree doesn't exist it will be created automatically.
    /// </summary>
    [HttpPost("api.user.tree.get")]
    [ProducesResponseType(typeof(TreeNodeDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionDTO), StatusCodes.Status500InternalServerError)]
    public IActionResult TreeGet([FromQuery][Required] string treeName)
    {
        try
        {
            _trees[treeName].GetTree(out var ftree);
            return StatusCode(StatusCodes.Status200OK, ftree);
        }
        catch (Exception ex)
        {
            return _logMessage.SetException(this, ex);
        }
    }
    
    /// <summary>
    /// Create a new node in your tree. You must to specify a parent node ID that belongs to your tree. A new node name must be unique across all siblings.
    /// </summary>
    [HttpPost("api.user.tree.node.create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionDTO), StatusCodes.Status500InternalServerError)]
    public IActionResult NodeCreate([FromQuery][Required] string treeName, [FromQuery][Required] long parentNodeId, [FromQuery][Required] string nodeName)
    {
        try
        {
            if (!_trees.CheckExist(treeName))
                return _logMessage.SetResult(this, OperationResult.ErrNoTree, treeName,parentNodeId, nodeName);
            
            OperationResult result = _trees[treeName].AddNode(parentNodeId, nodeName);
            
            return _logMessage.SetResult(this, result, treeName,parentNodeId, nodeName);
        }
        catch (Exception ex)
        {
            return _logMessage.SetException(this, ex);
        }
    }
    
    /// <summary>
    /// Delete an existing node in your tree. You must specify a node ID that belongs your tree.
    /// </summary>
    [HttpPost("api.user.tree.node.delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionDTO), StatusCodes.Status500InternalServerError)]
    public IActionResult NodeDelete([FromQuery][Required] string treeName, [FromQuery][Required] long nodeId)
    {
        try
        {
            OperationResult result = _trees[treeName].DeleteNode(nodeId);
            
            return _logMessage.SetResult(this, result, treeName, nodeId, null);
        }
        catch (Exception ex)
        {
            return _logMessage.SetException(this, ex);
        }
    }
    
    /// <summary>
    /// Rename an existing node in your tree. You must specify a node ID that belongs your tree. A new name of the node must be unique across all siblings.
    /// </summary>
    [HttpPost("api.user.tree.node.rename")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionDTO), StatusCodes.Status500InternalServerError)]
    public IActionResult NodeRename([FromQuery][Required] string treeName, [FromQuery][Required] long nodeId, [FromQuery][Required] string newNodeName)
    {
        try
        {
            OperationResult result = _trees[treeName].RenameNode(nodeId, newNodeName);
            
            return _logMessage.SetResult(this, result, treeName, nodeId, newNodeName);
        }
        catch (Exception ex)
        {
            return _logMessage.SetException(this, ex);
        }
    }
}
