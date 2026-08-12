namespace SharedLib.DTO
{
    public class RetestKomponentVadyDTO
    {
        public int KomponentId { get; set; }
        public string KomponentName { get; set; } = string.Empty;
        public List<RetestVadaDTO> Vady { get; set; } = new();
    }

    public class RetestVadaDTO
    {
        public int VadaId { get; set; }
        public int KomponentId { get; set; }
        public string VadaName { get; set; } = string.Empty;
    }
}