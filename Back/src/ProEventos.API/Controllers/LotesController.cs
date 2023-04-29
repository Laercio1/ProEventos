using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProEventos.Application.Contratos;
using Microsoft.AspNetCore.Http;
using ProEventos.Application.Dtos;
using ProEventos.Domain;

namespace ProEventos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LotesController : ControllerBase
    {
        private readonly ILoteService _loteService;

        public LotesController(ILoteService LoteService)
        {
            _loteService = LoteService;
        }

        /// <summary>
        /// Retornar informações de lote
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpGet("{eventoId}")]
        [ProducesResponseType(typeof(SucessRetorno<LoteDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> Get(int eventoId)
        {
            try
            {
                var lotes = await _loteService.GetLotesByEventoIdAsync(eventoId);
                if (lotes == null) return NoContent();

                return Ok(lotes);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar recuperar lotes. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Realizar atualização de lote
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <remarks>
        /// Exemplo de requisição
        /// 
        ///      PUT /api/Lotes/{eventoId}
        ///      [
        ///         {
        ///             "id": 0,
        ///             "nome": "Nome do lote",
        ///             "preco": 0,
        ///             "dataInicio": "Data de início da abertura do lote",
        ///             "dataFim": "Data final da abertura do lote",
        ///             "quantidade": 0,
        ///             "eventoId": 0
        ///         }
        ///      ]
        /// </remarks>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpPut("{eventoId}")]
        [ProducesResponseType(typeof(SucessRetorno<LoteDto>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> SaveLotes(int eventoId, LoteDto[] models)
        {
            try
            {
                var lotes = await _loteService.SaveLotes(eventoId, models);
                if (lotes == null) return NoContent();

                return Ok(lotes);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar salvar lotes. Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Realizar exclusão do lote
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        /// <response code="200">A solicitação foi bem-sucedida.</response>
        /// <response code="400">A solicitação enviada está incorreta ou mal formatada.</response>
        /// <response code="401">Não está autenticado e não tem permissão para acessar o recurso solicitado.</response>
        /// <response code="500">O servidor encontrou um erro interno ao processar a solicitação.</response>
        [HttpDelete("{eventoId}/{loteId}")]
        [ProducesResponseType(typeof(SucessRetorno<SucessRetornoDelete>), 200)]
        [ProducesResponseType(typeof(BadRequestRetorno), 400)]
        public async Task<IActionResult> Delete(int eventoId, int loteId)
        {
            try
            {
                var lote = await _loteService.GetLoteByIdsAsync(eventoId, loteId);
                if (lote == null) return NoContent();

                return await _loteService.DeleteLote(lote.EventoId, lote.Id)
                       ? Ok(new { message = "Lote Deletado" })
                       : throw new Exception("Ocorreu um problem não específico ao tentar deletar Lote.");
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Erro ao tentar deletar lotes. Erro: {ex.Message}");
            }
        }
    }
}
