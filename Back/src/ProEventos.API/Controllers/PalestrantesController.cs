using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProEventos.Application.Contratos;
using Microsoft.AspNetCore.Http;
using ProEventos.Application.Dtos;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using ProEventos.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using ProEventos.Persistence.Models;
using ProEventos.Domain;

namespace ProEventos.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PalestrantesController : ControllerBase
    {
        private readonly IPalestranteService _palestranteService;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IAccountService _accountService;

        public PalestrantesController(IPalestranteService palestranteService,
                                      IWebHostEnvironment hostEnvironment,
                                      IAccountService accountService)
        {
            _hostEnvironment = hostEnvironment;
            _accountService = accountService;
            _palestranteService = palestranteService;
        }

        /// <summary>
        /// Retornar informações de palestrante
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpGet("all")]
        [ProducesResponseType(typeof(SucessRetorno<PalestranteDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> GetAll([FromQuery]PageParams pageParams)
        {
            try
            {
                var palestrantes = await _palestranteService.GetAllPalestrantesAsync(pageParams, true);
                if (palestrantes == null) return NoContent();

                Response.AddPagination(palestrantes.CurrentPage,
                                       palestrantes.PageSize,
                                       palestrantes.TotalCount,
                                       palestrantes.TotalPages);

                return Ok(palestrantes);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar recuperar palestrantes! Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Retornar lista de palestrantes
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpGet()]
        [ProducesResponseType(typeof(SucessRetorno<PalestranteDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> GetPalestrantes()
        {
            try
            {
                var palestrante = await _palestranteService.GetPalestranteByUserIdAsync(User.GetUserId(), true);
                if (palestrante == null) return NoContent();

                return Ok(palestrante);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar recuperar palestrantes. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Realizar cadastro de um novo palestrante
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <remarks>
        /// Exemplo de requisição
        /// 
        ///      POST /api/Palestrantes
        ///      {
        ///         "miniCurriculo": "Descrição do currículo do palestrante"
        ///      }
        /// </remarks>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpPost]
        [ProducesResponseType(typeof(SucessRetorno<PalestranteDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> Post(PalestranteAddDto model)
        {
            try
            {
                var palestrante = await _palestranteService.GetPalestranteByUserIdAsync(User.GetUserId(), false);
                if (palestrante == null)
                    palestrante = await _palestranteService.AddPalestrantes(User.GetUserId(), model);

                return Ok(palestrante);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar adicionar eventos. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Realizar atualização de palestrante
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <remarks>
        /// Exemplo de requisição
        /// 
        ///      PUT /api/Palestrantes
        ///      {
        ///          "id": 0,
        ///          "miniCurriculo": "Descrição do currículo do palestrante"
        ///      }
        /// </remarks>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpPut]
        [ProducesResponseType(typeof(SucessRetorno<PalestranteDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> Put(PalestranteUpdateDto model)
        {
            try
            {
                var palestrante = await _palestranteService.UpdatePalestrante(User.GetUserId(), model);
                if (palestrante == null) return NoContent();

                return Ok(palestrante);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar atualizar eventos. Erro: {ex.Message}");
            }
        }
    }
}
