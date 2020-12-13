using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.Controllers
{
    [Authorize]
    [Route("api/produtos")]
    public class ProdutosController : MainController
    {

        private readonly IProdutoRepository _produtoRepository;
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;

        public ProdutosController(INotificador notificador, 
                                  IProdutoRepository produtoRepository,                                  
                                  IProdutoService produtoService,
                                  IMapper mapper,
                                  IUser user) : base(notificador, user)
        {
            _produtoRepository = produtoRepository;
            _produtoService = produtoService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<ProdutoViewModel>> ObterTodos()
        {
            return _mapper.Map<IEnumerable<ProdutoViewModel>>(await _produtoRepository.ObterProdutosFornecedores());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ObterPorId(Guid id)
        {
            var produtoViewModel = await ObterProduto(id);

            if (produtoViewModel == null) return NotFound();

            return produtoViewModel;
        }

        //[ClaimsAuthorize("Produto", "Adicionar")]
        [HttpPost]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar(ProdutoViewModel viewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imagemNome = Guid.NewGuid() + "_" + viewModel.Imagem;

            if (!UploadArquivo(viewModel.ImagemUpload, imagemNome))
            {
                return CustomResponse(viewModel);
            }

            viewModel.Imagem = imagemNome;

            await _produtoService.Adicionar(_mapper.Map<Produto>(viewModel));

            return CustomResponse(viewModel);
        }

        [RequestSizeLimit(40000000)] // limite de arquivos até 40mb
        [HttpPost("Adicionar")]
        public async Task<ActionResult<ProdutoViewModel>> AdicionarAlternativo(ProdutoImagemViewModel viewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imgPrefixo = Guid.NewGuid() + "_";

            if (! await UploadArquivoAlternativo(viewModel.ImagemUpload, imgPrefixo))
            {
                return CustomResponse(viewModel);
            }

            viewModel.Imagem = imgPrefixo + viewModel.ImagemUpload.FileName;

            await _produtoService.Adicionar(_mapper.Map<Produto>(viewModel));

            return CustomResponse(viewModel);
        }

        //[ClaimsAuthorize("Produto", "Atualizar")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> Atualizar(Guid id, ProdutoViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                NotificarErro("o Id informado não é o mesmo que foi passado na query");
                return CustomResponse(viewModel);
            }

            var produtoAtualizacao = await ObterProduto(id);
            viewModel.Imagem = produtoAtualizacao.Imagem;

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if (viewModel.ImagemUpload != null)
            {
                var imgNome = Guid.NewGuid() + "_" + viewModel.Imagem;

                if (!UploadArquivo(viewModel.ImagemUpload, imgNome))
                {
                    return CustomResponse(viewModel);
                }

                viewModel.Imagem = imgNome;
            }

            produtoAtualizacao.Nome = viewModel.Nome;
            produtoAtualizacao.Descricao = viewModel.Descricao;
            produtoAtualizacao.Valor = viewModel.Valor;
            produtoAtualizacao.Ativo = viewModel.Ativo;

            await _produtoService.Atualizar(_mapper.Map<Produto>(produtoAtualizacao));

            return CustomResponse(viewModel);
        }

        //[ClaimsAuthorize("Produto", "Adicionar")]
        //[HttpPost]
        //public async Task<ActionResult<ProdutoViewModel>> AdicionarAlternativo(ProdutoViewModel viewModel)
        //{
        //    if (!ModelState.IsValid) return CustomResponse(ModelState);

        //    var imgPrefixo = Guid.NewGuid() + "_";

        //    //if (! await UploadArquivoAlternativo(viewModel.ImagemUpload, imgPrefixo))
        //    //{
        //    //    return CustomResponse(viewModel);
        //    //}

        //    viewModel.Imagem = imgPrefixo + viewModel.ImagemUpload;

        //    await _produtoService.Adicionar(_mapper.Map<Produto>(viewModel));

        //    return CustomResponse(viewModel);
        //}

        //[ClaimsAuthorize("Fornecedor", "Excluir")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> Excluir(Guid id)
        {
            var produtoViewModel = await ObterProduto(id);

            if (produtoViewModel == null) return NotFound();

            await _produtoService.Remover(id);

            return CustomResponse(produtoViewModel);
        }

        private bool UploadArquivo(string arquivo, string imgNome)
        {

            if (string.IsNullOrEmpty(arquivo))
            {
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }

            var imgDataByteArray = Convert.FromBase64String(arquivo);

            var filePath = Path.Combine(@"C:\Users\cloud\source\repos\API_Rest\src\DevIO.Api", @"wwwroot\app\demo-webapi\src\assets", imgNome);

            if (System.IO.File.Exists(filePath))
            {
                NotificarErro("Ja existe um arquivo com este nome!");
                return false;
            }

            System.IO.File.WriteAllBytes(filePath, imgDataByteArray);

            return true;
        }

        private async Task<bool> UploadArquivoAlternativo(IFormFile arquivo, string imgPrefixo)
        {

            if (arquivo == null || arquivo.Length == 0)
            {
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }

            var filePath = Path.Combine(@"C:\Users\cloud\source\repos\API_Rest\src\DevIO.Api", @"wwwroot\app\demo-webapi\src\assets", imgPrefixo + arquivo.FileName);

            if (System.IO.File.Exists(filePath))
            {
                NotificarErro("Ja existe um arquivo com este nome!");
                return false;
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            return true;
        }

        private async Task<ProdutoViewModel> ObterProduto(Guid id)
        {
            return _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObterProdutoFornecedor(id));
        }
    }
}