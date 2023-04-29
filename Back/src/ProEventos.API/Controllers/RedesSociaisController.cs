using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProEventos.Application.Contratos;
using Microsoft.AspNetCore.Http;
using ProEventos.Application.Dtos;
using ProEventos.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using ProEventos.Domain;

namespace ProEventos.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RedesSociaisController : ControllerBase
    {
        private readonly IRedeSocialService _redeSocialService;
        private readonly IEventoService _eventoService;
        private readonly IPalestranteService _palestranteService;

        public RedesSociaisController(IRedeSocialService RedeSocialService,
                                      IEventoService eventoService,
                                      IPalestranteService palestranteService)
        {
            _palestranteService = palestranteService;
            _redeSocialService = RedeSocialService;
            _eventoService = eventoService;
        }

        /// <summary>
        /// Retornar informações de rede social do evento
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpGet("evento/{eventoId}")]
        [ProducesResponseType(typeof(SucessRetorno<RedeSocialDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> GetByEvento(int eventoId)
        {
            try
            {
                if (!(await AutorEvento(eventoId)))
                    return Unauthorized();

                var redeSocial = await _redeSocialService.GetAllByEventoIdAsync(eventoId);
                if (redeSocial == null) return NoContent();

                return Ok(redeSocial);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar recuperar Rede Social por Evento! Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Retornar informações de rede social do palestrante
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpGet("palestrante")]
        [ProducesResponseType(typeof(SucessRetorno<RedeSocialDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> GetByPalestrante()
        {
            try
            {
                var palestrante = await _palestranteService.GetPalestranteByUserIdAsync(User.GetUserId());
                if (palestrante == null) return Unauthorized();

                var redeSocial = await _redeSocialService.GetAllByPalestranteIdAsync(palestrante.Id);
                if (redeSocial == null) return NoContent();

                return Ok(redeSocial);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar recuperar Rede Social por Palestrante. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Realizar atualização de rede social do evento
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <remarks>
        /// Exemplo de requisição
        /// 
        ///      PUT /api/RedesSociais/evento/{eventoId}
        ///      {
        ///          "id": 0,
        ///          "nome": "Nome da rede social",
        ///          "url": "URL da rede social",
        ///          "eventoId": 0,
        ///      }
        /// </remarks>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpPut("evento/{eventoId}")]
        [ProducesResponseType(typeof(SucessRetorno<RedeSocialDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> SaveByEvento(int eventoId, RedeSocialDto[] models)
        {
            try
            {
                if (!(await AutorEvento(eventoId)))
                    return Unauthorized();

                var redeSocial = await _redeSocialService.SaveByEvento(eventoId, models);
                if (redeSocial == null) return NoContent();

                return Ok(redeSocial);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar salvar Rede Social por Evento. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Realizar atualização de rede social do palestrante
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <remarks>
        /// Exemplo de requisição
        /// 
        ///      PUT /api/RedesSociais/palestrante
        ///      {
        ///          "id": 0,
        ///          "nome": "Nome da rede social",
        ///          "url": "URL da rede social",
        ///          "palestranteId": 0,
        ///      }
        /// </remarks>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpPut("palestrante")]
        [ProducesResponseType(typeof(SucessRetorno<RedeSocialDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> SaveByPalestrante(RedeSocialDto[] models)
        {
            try
            {
                var palestrante = await _palestranteService.GetPalestranteByUserIdAsync(User.GetUserId());
                if (palestrante == null) return Unauthorized();

                var redeSocial = await _redeSocialService.SaveByPalestrante(palestrante.Id, models);
                if (redeSocial == null) return NoContent();

                return Ok(redeSocial);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar salvar Rede Social por Palestrante. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Realizar exclusão da rede social do evento
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpDelete("evento/{eventoId}/{redeSocialId}")]
        [ProducesResponseType(typeof(SucessRetorno<SucessRetornoDelete>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> DeleteByEvento(int eventoId, int redeSocialId)
        {
            try
            {
                if (!(await AutorEvento(eventoId)))
                    return Unauthorized();

                var RedeSocial = await _redeSocialService.GetRedeSocialEventoByIdsAsync(eventoId, redeSocialId);
                if (RedeSocial == null) return NoContent();

                return await _redeSocialService.DeleteByEvento(eventoId, redeSocialId) 
                       ? Ok(new { message = "Rede Social Deletada" }) 
                       : throw new Exception("Ocorreu um problem não específico ao tentar deletar Rede Social por Evento.");
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar deletar Rede Social por Evento. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Realizar exclusão da rede social do palestrante
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpDelete("palestrante/{redeSocialId}")]
        [ProducesResponseType(typeof(SucessRetorno<SucessRetornoDelete>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> DeleteByPalestrante(int redeSocialId)
        {
            try
            {
                var palestrante = await _palestranteService.GetPalestranteByUserIdAsync(User.GetUserId());
                if (palestrante == null) return Unauthorized();

                var RedeSocial = await _redeSocialService.GetRedeSocialPalestranteByIdsAsync(palestrante.Id, redeSocialId);
                if (RedeSocial == null) return NoContent();

                return await _redeSocialService.DeleteByPalestrante(palestrante.Id, redeSocialId) 
                       ? Ok(new { message = "Rede Social Deletada" }) 
                       : throw new Exception("Ocorreu um problem não específico ao tentar deletar Rede Social por Palestrante.");
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar deletar Rede Social por Palestrante. Erro: {ex.Message}");
            }
        }

        [NonAction]
        private async Task<bool> AutorEvento(int eventoId)
        {
            var evento = await _eventoService.GetEventoByIdAsync(User.GetUserId(), eventoId, false);
            if (evento == null) return false;

            return true;
        }
    }
}
