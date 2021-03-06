// ***********************************************************************
// Assembly         : ACBr.Net.DFe.Core
// Author           : RFTD
// Created          : 06-30-2018
//
// Last Modified By : RFTD
// Last Modified On : 06-30-2018
// ***********************************************************************
// <copyright file="DFeResposta.cs" company="ACBr.Net">
//		        		   The MIT License (MIT)
//	     		    Copyright (c) 2016 Grupo ACBr.Net
//
//	 Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//	 The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//	 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ACBr.Net.Core;
using ACBr.Net.Core.Extensions;
using ACBr.Net.DFe.Core.Common;

namespace ACBr.Net.DFe.Core.Service
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public abstract class DFeServiceClient<TDFeConfig, TParent, TGeralConfig, TVersaoDFe, TWebserviceConfig,
        TCertificadosConfig, TArquivosConfig, TSchemas, TService> : DFeSoap12ServiceClientBase<TService>
        where TDFeConfig : DFeConfigBase<TParent, TGeralConfig, TVersaoDFe, TWebserviceConfig, TCertificadosConfig, TArquivosConfig, TSchemas>
        where TParent : ACBrComponent
        where TGeralConfig : DFeGeralConfigBase<TParent, TVersaoDFe>
        where TVersaoDFe : Enum
        where TWebserviceConfig : DFeWebserviceConfigBase<TParent>
        where TCertificadosConfig : DFeCertificadosConfigBase<TParent>
        where TArquivosConfig : DFeArquivosConfigBase<TParent, TSchemas>
        where TSchemas : Enum
        where TService : class
    {
        #region Fields

        protected readonly object serviceLock;

        #endregion Fields

        #region Constructors

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="config"></param>
        /// <param name="url"></param>
        /// <param name="certificado"></param>
        protected DFeServiceClient(TDFeConfig config, string url, X509Certificate2 certificado = null) :
            base(url, config.WebServices.TimeOut, certificado)
        {
            serviceLock = new object();
            Configuracoes = config;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///
        /// </summary>
        public TDFeConfig Configuracoes { get; }

        /// <summary>
        ///
        /// </summary>
        public TSchemas Schema { get; protected set; }

        /// <summary>
        ///
        /// </summary>
        public string ArquivoEnvio { get; protected set; }

        /// <summary>
        ///
        /// </summary>
        public string ArquivoResposta { get; protected set; }

        /// <summary>
        ///
        /// </summary>
        public string EnvelopeSoap { get; protected set; }

        /// <summary>
        ///
        /// </summary>
        public string RetornoWS { get; protected set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Fun��o para validar a menssagem a ser enviada para o webservice.
        /// </summary>
        /// <param name="xml"></param>
        protected virtual void ValidateMessage(string xml)
        {
            ValidateMessage(xml, Schema);
        }

        /// <summary>
        /// Fun��o para validar a menssagem a ser enviada para o webservice.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="schema"></param>
        protected virtual void ValidateMessage(string xml, TSchemas schema)
        {
            var schemaFile = Configuracoes.Arquivos.GetSchema(schema);
            XmlSchemaValidation.ValidarXml(xml, schemaFile, out var erros, out _);

            Guard.Against<ACBrDFeValidationException>(erros.Any(), "Erros de valida��o do xml." +
                                                                   $"{(Configuracoes.Geral.ExibirErroSchema ? Environment.NewLine + erros.AsString() : "")}");
        }

        /// <summary>
        /// Salvar o arquivo xml no disco de acordo com as propriedades.
        /// </summary>
        /// <param name="conteudoArquivo"></param>
        /// <param name="nomeArquivo"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        protected void GravarSoap(string conteudoArquivo, string nomeArquivo)
        {
            if (Configuracoes.WebServices.Salvar == false) return;

            nomeArquivo = Path.Combine(Configuracoes.Arquivos.PathSalvar, nomeArquivo);
            File.WriteAllText(nomeArquivo, conteudoArquivo, Encoding.UTF8);
        }

        /// <inheritdoc />
        protected override void BeforeSendDFeRequest(string message)
        {
            EnvelopeSoap = message;
            GravarSoap(message, $"{DateTime.Now:yyyyMMddHHmmssfff}_{ArquivoEnvio}_env.xml");
        }

        /// <inheritdoc />
        protected override void AfterReceiveDFeReply(string message)
        {
            RetornoWS = message;
            GravarSoap(message, $"{DateTime.Now:yyyyMMddHHmmssfff}_{ArquivoResposta}_ret.xml");
        }

        #endregion Methods
    }
}