using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using WebTree.Logic;

namespace WebTree.Api;

[ApiController]
public class UserJournalController : ControllerBase
{
    private readonly LogReader _logReader;

    public UserJournalController(LogReader logReader)
    {
        _logReader = logReader;
    }

    [HttpPost("api.user.journal.getRange")]
    [ProducesResponseType(typeof(JournalShortDataDTO), StatusCodes.Status200OK)]
    public IActionResult GetRange([FromQuery][Required] int skip, [FromQuery][Required] int take, [FromBody] JournalFilterDTO filter)
    {
        try
        {
            JournalShortDataDTO data = _logReader.GetRange(skip, take, filter?.From, filter?.To, filter?.Search);
        
            return StatusCode(StatusCodes.Status200OK, data);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex);
        }
    }
    
    [HttpPost("api.user.journal.getSingle")]
    [ProducesResponseType(typeof(JournalFullElementDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetSingle([FromQuery][Required] long id)
    {
        try
        {
            JournalFullElementDTO data = _logReader.GetSingle(id);
            return data!=null ? StatusCode(StatusCodes.Status200OK, data) : StatusCode(StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex);
        }
    }
}