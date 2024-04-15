import {Component, OnInit} from '@angular/core';
import {SchemaDescriptionDto, SchemaDescriptionFileDto, SchemaDescriptionService} from "../../../generated/client";
import {NgFor, NgIf} from "@angular/common";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {
  faDownload,
  faLanguage,
  faPlus,
  faTrashCan,
  faTriangleExclamation,
  faUpload
} from "@fortawesome/free-solid-svg-icons";
import {NgbModal, NgbPopover} from "@ng-bootstrap/ng-bootstrap";
import {AddFileModalComponent, AddFileModalResult} from "./add-file-modal/add-file-modal.component";
import {InputObjectNameModalComponent} from "../../shared/input-object-name-modal/input-object-name-modal.component";
import {
  DeleteConfirmationModalComponent
} from "../../shared/delete-confirmation-modal/delete-confirmation-modal.component";
import {PermissionsService} from "../../../authorization/permissions.service";
import { ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

enum Dbms {
  Oracle = 'Oracle',
  SqlServer = 'SqlServer',
}

@Component({
  selector: 'app-schemas',
  standalone: true,
  imports: [
    NgFor,
    FaIconComponent,
    NgIf,
    NgbPopover,
    TranslateModule,
  ],
  templateUrl: './schemas.component.html',
  styleUrl: './schemas.component.css'
})
export class SchemasComponent implements OnInit {
  public schemas: Array<SchemaDescriptionDto> = [];
  public availableDbms: Array<string> = [
    Dbms.Oracle,
    Dbms.SqlServer,
  ];

  public constructor(
    private schemaDescriptionService: SchemaDescriptionService,
    private modalService: NgbModal,
    public permissionsService: PermissionsService,
    private activatedRoute: ActivatedRoute,
  ) { }

  private fetchSchemas() {
    this.schemaDescriptionService.apiSchemaDescriptionsGet(`contestId==${this.activatedRoute.snapshot.params.contestId}`).subscribe(schemas => {
      this.schemas = schemas.schemaDescriptions || [];
    });
  }

  public ngOnInit() {
    this.fetchSchemas();
  }

  public upload(schemaId: string, dbms: string) {
    const reader = new FileReader();
    reader.onload = () => {
      const description = reader.result as string;
      this.schemaDescriptionService.apiSchemaDescriptionsSchemaDescriptionIdFilesDbmsPut(schemaId, dbms, {description}).subscribe(() => {
        this.fetchSchemas();
      });
    }
    const fileInput = document.createElement('input');
    fileInput.type = 'file';
    fileInput.onchange = () => {
      const file = fileInput.files?.item(0);
      if (file) {
        reader.readAsText(file);
      }
    }
    fileInput.click();
  }

  public download(file: SchemaDescriptionFileDto, schema: SchemaDescriptionDto) {
    const blob = new Blob([file.description ?? ''], {type: 'text/plain'});
    const url = window.URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = `${schema.name}-${file.dbms}.sql`;
    anchor.click();
  }

  public addFile(schemaId: string) {
    const modalRef = this.modalService.open(AddFileModalComponent);
    modalRef.componentInstance.disallowedTargetDbms = this.schemas.find(s => s.id === schemaId)?.files?.map(f => f.dbms) ?? [];
    modalRef.result.then((result: AddFileModalResult | undefined) => {
      if (!result?.description && !result?.sourceDbms) return;
      this.schemaDescriptionService.apiSchemaDescriptionsSchemaDescriptionIdFilesPost(schemaId, {
        dbms: result.dbms,
        description: result.description,
        sourceDbms: result.sourceDbms,
      }).subscribe(() => {
        this.fetchSchemas();
      });
    });
  }

  public addSchema() {
    const modalRef = this.modalService.open(InputObjectNameModalComponent);
    modalRef.componentInstance.title = 'Create new schema';
    modalRef.result.then((result: string | undefined) => {
      if (!result) return;
      const contestId = this.activatedRoute.snapshot.params.contestId;
      this.schemaDescriptionService.apiSchemaDescriptionsPost({name: result, contestId}).subscribe(() => {
        this.fetchSchemas();
      });
    });
  }

  public deleteSchema(schemaId: string) {
    const modalRef = this.modalService.open(DeleteConfirmationModalComponent);
    modalRef.result.then((result: string | undefined) => {
      if (!result) return;
      this.schemaDescriptionService.apiSchemaDescriptionsSchemaDescriptionIdDelete(schemaId).subscribe(() => {
        this.fetchSchemas();
      });
    });
  }

  public deleteFile(schemaId: string, dbms: string) {
    const modalRef = this.modalService.open(DeleteConfirmationModalComponent);
    modalRef.result.then((result: string | undefined) => {
      if (!result) return;
      this.schemaDescriptionService.apiSchemaDescriptionsSchemaDescriptionIdFilesDbmsDelete(schemaId, dbms).subscribe(() => {
        this.fetchSchemas();
      });
    });
  }

  public canAddSomeFile(schema: SchemaDescriptionDto): boolean {
    return !this.availableDbms.every(dbms => schema.files?.some(file => file.dbms === dbms));
  }

  protected readonly faDownload = faDownload;
  protected readonly faUpload = faUpload;
  protected readonly faLanguage = faLanguage;
  protected readonly faPlus = faPlus;
  protected readonly faTrashCan = faTrashCan;
  protected readonly faTriangleExclamation = faTriangleExclamation;
}
