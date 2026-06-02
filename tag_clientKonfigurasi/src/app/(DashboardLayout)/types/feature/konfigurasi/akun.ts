export interface AkunList {
    Id : string;
    UserName : string;
    Email : string;
    NoKtp?: string;
    FullName : string;
    Photo : string;
    PhoneNumber : string;
    Cabang : string;
    Group: string[]
    Active : boolean;
}
