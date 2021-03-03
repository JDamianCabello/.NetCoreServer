using System;

/*
 * 
 * Modelo de llamadas para la comunicacion cliente-servidor
 * 
 */
namespace SharedNameSpace //Siempre en objetos compartidos usamos el mismo namespace
{
    [Serializable]
    class Call{
        public User to;
        public string number;
        public string campaigne;
        public string callid;
        public int indice;


        /// <summary>
        /// Crea un objeto Call
        /// </summary>
        /// <param name="to">Usuario al que va dirijida la llamada</param>
        /// <param name="number">Número de telefono de la llamada</param>
        /// <param name="campaigne">Campania de la llamada</param>
        /// <param name="callid">Id de la llamada [Este campo se puso porque era necesario para vocalcom]</param>
        /// <param name="indice">Indice de la llamada [Este campo se puso porque era necesario para vocalcom]</param>
        public Call(User to, string number, string campaigne, string callid, int indice){
            this.campaigne = campaigne;
            this.number = number;
            this.to = to;
            this.callid = callid;
            this.indice = indice;
        }

    }
}