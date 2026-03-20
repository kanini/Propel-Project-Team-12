# Controllers Folder

This folder will contain Web API controller classes that handle HTTP requests and responses.

Controllers should:
- Use attribute routing
- Apply [ApiController] attribute
- Return IActionResult or ActionResult<T>
- Inject services via constructor
- Contain minimal logic (orchestration only)
