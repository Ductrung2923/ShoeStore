namespace PjPRN222.Services
{
    public class GhnService
    {
        private readonly HttpClient _httpClient;

        public GhnService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("GHN");
        }

        public async Task<string> GetProvincesAsync()
        {
            var response = await _httpClient.GetAsync("/shiip/public-api/master-data/province");
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetDistrictAsync(int provinceId)
        {
            var content = new StringContent($"{{\"province_id\": {provinceId}}}", System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/shiip/public-api/master-data/district", content);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetWardAsync(int districtId)
        {
            var content = new StringContent($"{{\"district_id\": {districtId}}}", System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/shiip/public-api/master-data/ward", content);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
