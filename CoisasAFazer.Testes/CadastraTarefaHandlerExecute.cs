using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

            var mock = new Mock<ILogger<CadastraTarefaHandler>>();

            var options = new DbContextOptionsBuilder<DbTarefasContext>()
                .UseInMemoryDatabase("DbTarefasContext")
                .Options;

            var contexto = new DbTarefasContext(options);
            var repo = new RepositorioTarefa(contexto);

            var handler = new CadastraTarefaHandler(repo, mock.Object);

            // act 
            handler.Execute(comando);

            // assert
            var tarefas = repo.ObtemTarefas(t => t.Titulo == "Estudar xunit").FirstOrDefault();
            Assert.NotNull(tarefas);
        }

        delegate void CapturaMensagemDeLog(LogLevel level, EventId eventId, object state, Exception exception,
            Func<object, Exception, string> function);

        [Fact]
        public void DadaTarefaComInformacoesValidasDeveLogar()
        {
            // arrange
            var comando = new CadastraTarefa("Estudar xunit", new Categoria("Estudo"), new DateTime(2019, 12, 31));

            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            LogLevel levelCapturado = LogLevel.Error;
            string mensagemCapturada = string.Empty;

            CapturaMensagemDeLog captura = (level, eventId, state, exception, func) =>
            {
                levelCapturado = level;
                mensagemCapturada = func(state, exception);
            };

            mockLogger.Setup(l =>
                l.Log(
                    It.IsAny<LogLevel>(),  // nivel de erro de log = > logError
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()
                )).Callback(captura);

            var mockTarefas = new Mock<IRepositorioTarefas>();

            var handler = new CadastraTarefaHandler(mockTarefas.Object, mockLogger.Object);

            // act 
            handler.Execute(comando);

            // assert
            Assert.Equal(LogLevel.Debug, levelCapturado);
            Assert.Contains("Estudar xunit", mensagemCapturada);
            
        }

        [Fact]
        public void QuandoExceptionForLancadaResultadoIsSuccessDeveSerFalso()
        {
            // arrange
            var comando = new CadastraTarefa("Estudar xunit", new Categoria("Estudo"), new DateTime(2019, 12, 31));

            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            var mock = new Mock<IRepositorioTarefas>();

            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>()))
                .Throws(new Exception("Houve um erro na inclusão de tarefas"));

            var repo = mock.Object;

            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);

            // act 
            CommandResult resultado = handler.Execute(comando);

            // assert
            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void QuandoExceptionForLancadaDeveLogarAMensagemDaExcecao()
        {
            // arrange
            var mensagemDeErroEsperada = "Houve um erro na inclusão de tarefas";
            var excecaoEsperada = new Exception(mensagemDeErroEsperada);

            var comando = new CadastraTarefa("Estudar xunit", new Categoria("Estudo"), new DateTime(2019, 12, 31));

            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            var mock = new Mock<IRepositorioTarefas>();

            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>()))
                .Throws(excecaoEsperada);

            var repo = mock.Object;

            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);

            // act 
            CommandResult resultado = handler.Execute(comando);

            // assert
            mockLogger.Verify(l => 
                l.Log(
                    LogLevel.Error,  // nivel de erro de log = > logError
                    It.IsAny<EventId>(), 
                    It.IsAny<object>(), 
                    excecaoEsperada, 
                    It.IsAny<Func<object, Exception, string>>()
                ),
                Times.Once());
        }
    }
}
