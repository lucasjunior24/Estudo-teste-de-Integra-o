using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Microsoft.EntityFrameworkCore;
using Moq;
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

            var options = new DbContextOptionsBuilder<DbTarefasContext>()
                .UseInMemoryDatabase("DbTarefasContext")
                .Options;

            var contexto = new DbTarefasContext(options);
            var repo = new RepositorioTarefa(contexto);

            var handler = new CadastraTarefaHandler(repo);

            // act 
            handler.Execute(comando);

            // assert
            var tarefas = repo.ObtemTarefas(t => t.Titulo == "Estudar xunit").FirstOrDefault();
            Assert.NotNull(tarefas);
        }

        [Fact]
        public void QuandoExceptionForLancadaResultadoIsSuccessDeveSerFalso()
        {
            // arrange
            var comando = new CadastraTarefa("Estudar xunit", new Categoria("Estudo"), new DateTime(2019, 12, 31));

            var mock = new Mock<IRepositorioTarefas>();

            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>()))
                .Throws(new Exception("Houve um erro na inclusão de tarefas"));

            var repo = mock.Object;

            var handler = new CadastraTarefaHandler(repo);

            // act 
            CommandResult resultado = handler.Execute(comando);

            // assert
            Assert.False(resultado.IsSuccess);
        }
    }
}
