using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Services.Handlers;
using System;
using System.Linq;
using Xunit;

namespace CoisasAFazer.Testes
{
    public class CadastraTarefaHandlerExecute
    {
        [Fact]
        public void DadaTarefaComInformacoesValidasDeveIncluirNoBD()
        {
            // arrange
            var comando = new CadastraTarefa("Estudar xunit", new Categoria("Estudo"), new DateTime(2019, 12, 31));

            var repo = new RepositorioFake();

            var handler = new CadastraTarefaHandler(repo);

            // act 
            handler.Execute(comando);

            // assert
            var tarefas = repo.ObtemTarefas(t => t.Titulo == "Estudar xunit").FirstOrDefault();
            Assert.NotNull(tarefas);
        }
    }
}
