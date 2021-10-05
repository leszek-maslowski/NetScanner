import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {
  public hosts: ScannerHost[];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<ScannerHost[]>(baseUrl + 'scannerhost').subscribe(result => {
      this.hosts = result;
    }, error => console.error(error));
  }
}

interface ScannerHost {
  address: string;
  name: string;
  os: string;
  computerName: string;
}
